using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BigBeerData.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;

namespace BigBeerData.Api.Ingestion
{
    public class Ingestion
    {
        private readonly ILogger _logger;
        private readonly BigBeerContext _context;
        private readonly HttpClient _httpClient;

        public Ingestion(ILoggerFactory loggerFactory, BigBeerContext context, IHttpClientFactory httpClientFactory)
        {
            _logger = loggerFactory.CreateLogger<Ingestion>();
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }



        [Function("IngestionHttp")]
        public async Task<HttpResponseData> RunHttp([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("HTTP Ingestion Triggered.");

            // Parse query parameter or body
            string? location = req.Query["location"];
            if (string.IsNullOrEmpty(location))
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic? data = JsonConvert.DeserializeObject(requestBody);
                location = data?.location;
            }

            if (string.IsNullOrEmpty(location))
            {
                var badRes = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badRes.WriteStringAsync("Please pass a 'location' in the query string or request body.");
                return badRes;
            }

            await RunIngestion(location);

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync($"Ingestion triggered for location: {location}");
            return response;
        }

        private async Task RunIngestion(string locationQuery)
        {
            _logger.LogInformation($"Starting Ingestion for: {locationQuery}");

            string? clientId = Environment.GetEnvironmentVariable("UntappdClientId");
            string? clientSecret = Environment.GetEnvironmentVariable("UntappdClientSecret");
            string baseUrl = "https://api.untappd.com/v4/";

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogError("Untappd credentials (UntappdClientId, UntappdClientSecret) not set.");
                return;
            }

            // 1. Ensure Target Location exists (e.g. Melbourne)
            // Note: If locationQuery is different, we might want to map it to a Location entity.
            // For now, we reuse the "Melbourne" logic only if query matches, or we create a new one?
            // User asked to "replace Melbourne", so implies we act on that location.
            // Simple approach: Check/Create Location based on the query string (naive split or just name).
            // Let's use the full query string as the LocationName for now to avoid duplicates.

            var targetLocation = await _context.Locations.FirstOrDefaultAsync(l => l.LocationName == locationQuery);
            if (targetLocation == null)
            {
                // Find next ID (naive)
                int maxId = await _context.Locations.MaxAsync(l => (int?)l.LocationId) ?? 0;
                targetLocation = new Location
                {
                    LocationId = maxId + 1,
                    LocationName = locationQuery
                };
                _context.Locations.Add(targetLocation);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Created '{locationQuery}' location with ID {targetLocation.LocationId}.");
            }

            // 2. Query Untappd Search API
            int offset = 0;
            int limit = 50;
            int totalFetched = 0;
            int maxToFetch = 100; // Limit

            string searchQ = Uri.EscapeDataString(locationQuery);

            while (totalFetched < maxToFetch)
            {
                string searchUrl = $"{baseUrl}search/venue?q={searchQ}&client_id={clientId}&client_secret={clientSecret}&limit={limit}&offset={offset}";
                _logger.LogInformation($"Fetching: {searchUrl}");

                var response = await _httpClient.GetAsync(searchUrl);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API Error: {response.StatusCode}");
                    break;
                }

                string json = await response.Content.ReadAsStringAsync();
                UntappdSearchVenueRoot? root;
                try
                {
                    root = JsonConvert.DeserializeObject<UntappdSearchVenueRoot>(json);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Deserialization failed.");
                    break;
                }

                if (root?.Response?.Venues?.Items == null || root.Response.Venues.Items.Count == 0)
                {
                    _logger.LogInformation("No more venues found.");
                    break;
                }

                foreach (var item in root.Response.Venues.Items)
                {
                    var v = item.Venue;
                    if (v == null) continue;

                    // Filter Logic - Relax strict "Australia" filter if user queries elsewhere?
                    // User said "replace Melbourne", so likely they want to query other places. 
                    // We should probably remove the strict "Australia/VIC" filter or adapt it.
                    // Let's REMOVE the strict filter to allow other locations as requested.

                    if (v.Categories?.Items != null)
                    {
                        var excludedCategories = new HashSet<string> { "Plane", "Airport", "Office", "Home (private)", "Travel & Transport", "Travel and Transportation" };
                        if (v.Categories.Items.Any(c => c.CategoryName != null && excludedCategories.Contains(c.CategoryName)))
                        {
                            continue;
                        }
                    }

                    var existing = await _context.Establishments.FindAsync(v.Venue_id);

                    // Fetch Details
                    string detailUrl = $"{baseUrl}venue/info/{v.Venue_id}?client_id={clientId}&client_secret={clientSecret}";
                    var detailRes = await _httpClient.GetAsync(detailUrl);
                    if (!detailRes.IsSuccessStatusCode) continue;

                    string detailJson = await detailRes.Content.ReadAsStringAsync();
                    var detailRoot = JsonConvert.DeserializeObject<UntappdVenueInfoRoot>(detailJson);
                    var detailVenue = detailRoot?.Response?.Venue;

                    if (detailVenue == null || detailVenue.IsClosed) continue;

                    if (existing == null)
                    {
                        var est = new Establishment
                        {
                            EstablishmentId = detailVenue.VenueId,
                            EstablishmentName = detailVenue.VenueName,
                            Category = detailVenue.Categories?.Items?.FirstOrDefault()?.CategoryName,
                            Lat = detailVenue.Location?.Lat ?? 0,
                            Long = detailVenue.Location?.Lng ?? 0,
                            LocationId = targetLocation.LocationId,
                            BaseZoom = 10,
                            MaxedCheckinHistory = false
                        };

                        if (string.IsNullOrEmpty(targetLocation.State) && !string.IsNullOrEmpty(detailVenue.Location?.VenueState))
                        {
                            targetLocation.State = detailVenue.Location.VenueState;
                            targetLocation.Country = detailVenue.Location.VenueCountry;
                        }

                        _context.Establishments.Add(est);
                        _logger.LogInformation($"Added: {est.EstablishmentName}");
                    }
                    else
                    {
                        // Update
                        if (Math.Abs(existing.Lat - (detailVenue.Location?.Lat ?? 0)) > 0.0001 ||
                             Math.Abs(existing.Long - (detailVenue.Location?.Lng ?? 0)) > 0.0001)
                        {
                            existing.Lat = detailVenue.Location?.Lat ?? 0;
                            existing.Long = detailVenue.Location?.Lng ?? 0;
                        }
                        if (string.IsNullOrEmpty(existing.Category) && detailVenue.Categories?.Items?.Any() == true)
                        {
                            existing.Category = detailVenue.Categories.Items.First().CategoryName;
                        }
                    }
                    // Rate Limit
                    await Task.Delay(2000);
                }

                await _context.SaveChangesAsync();
                totalFetched += root.Response.Venues.Items.Count;
                offset += root.Response.Venues.Items.Count;

                await Task.Delay(1000);
            }
            _logger.LogInformation("Ingestion Run Complete.");
        }
    }

    // --- Minimal Models ---
    public class UntappdSearchVenueRoot { public UntappdSearchVenueResponse? Response { get; set; } }
    public class UntappdSearchVenueResponse { public UntappdSearchVenueList? Venues { get; set; } }
    public class UntappdSearchVenueList { public List<UntappdSearchVenueItem>? Items { get; set; } }
    public class UntappdSearchVenueItem { public UntappdVenue? Venue { get; set; } }
    public class UntappdVenue
    {
        [JsonProperty("venue_id")] public int Venue_id { get; set; }
        [JsonProperty("location")] public string? Location { get; set; }
        [JsonProperty("categories")] public UntappdVenueCategories? Categories { get; set; }
    }
    public class UntappdVenueCategories { [JsonProperty("items")] public List<UntappdVenueCategoryItem>? Items { get; set; } }
    public class UntappdVenueCategoryItem { [JsonProperty("category_name")] public string? CategoryName { get; set; } }

    public class UntappdVenueInfoRoot { [JsonProperty("response")] public UntappdVenueInfoResponse? Response { get; set; } }
    public class UntappdVenueInfoResponse { [JsonProperty("venue")] public UntappdVenueDetail? Venue { get; set; } }
    public class UntappdVenueDetail
    {
        [JsonProperty("venue_id")] public int VenueId { get; set; }
        [JsonProperty("venue_name")] public string? VenueName { get; set; }
        [JsonProperty("is_closed")] public bool IsClosed { get; set; }
        [JsonProperty("categories")] public UntappdVenueCategories? Categories { get; set; }
        [JsonProperty("location")] public UntappdVenueDetailLocation? Location { get; set; }
    }
    public class UntappdVenueDetailLocation
    {
        [JsonProperty("lat")] public double Lat { get; set; }
        [JsonProperty("lng")] public double Lng { get; set; }
        [JsonProperty("venue_state")] public string? VenueState { get; set; }
        [JsonProperty("venue_country")] public string? VenueCountry { get; set; }
    }
}
