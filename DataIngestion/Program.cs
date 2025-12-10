using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BigBeerData.Shared;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DataIngestion
{
    class Program
    {
        private static IConfiguration Configuration;

        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            string ClientId = Configuration["Untappd:ClientId"];
            string ClientSecret = Configuration["Untappd:ClientSecret"];
            string ConnectionString = Configuration["ConnectionStrings:DefaultConnection"];

            if (string.IsNullOrEmpty(ClientId) || string.IsNullOrEmpty(ClientSecret))
            {
                Console.WriteLine("Error: Untappd ClientId or ClientSecret is missing in appsettings.json");
                return;
            }

            Console.WriteLine("Starting Untappd Data Ingestion...");

            var optionsBuilder = new DbContextOptionsBuilder<BigBeerContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using var context = new BigBeerContext(optionsBuilder.Options);

            // 1. Ensure "Melbourne" Location exists
            var melbourneLocation = await context.Locations.FirstOrDefaultAsync(l => l.LocationName == "Melbourne");
            if (melbourneLocation == null)
            {
                Console.WriteLine("Creating 'Melbourne' location...");
                // Find a new ID - max + 1
                int newId = 1;
                if (await context.Locations.AnyAsync())
                {
                    newId = await context.Locations.MaxAsync(l => l.LocationId) + 1;
                }

                melbourneLocation = new Location
                {
                    LocationId = newId,
                    LocationName = "Melbourne"
                };
                context.Locations.Add(melbourneLocation);
                await context.SaveChangesAsync();
                Console.WriteLine($"Created 'Melbourne' with ID {melbourneLocation.LocationId}");
            }
            else
            {
                Console.WriteLine($"Found 'Melbourne' location with ID {melbourneLocation.LocationId}");
            }

            // 2. Fetch Venues
            using var httpClient = new HttpClient();
            // User-Agent is required by Untappd
            httpClient.DefaultRequestHeaders.Add("User-Agent", "BigBeerDataImporter/1.0 (" + ClientId + ")");

            string query = "Melbourne+Australia";
            // Check if we want to add Australia to query? The user said "Melbourne Victoria" but search term "Melbourne" seems to work.
            // Let's stick to "Melbourne" as per the user's initial prompt "find venues that are in melbourne victoria"
            // The search API response showed "Melbourne, VIC" in location field, so query "Melbourne" is fine.

            // Note: Public API search/venue usually returns limited results (e.g. 25 or 50). Pagination might be needed but
            // search endpoints often don't support deep pagination for free tiers. We will try one hit first.
            // The previous curl output showed 7110 found but only 25 returned.

            // We will loop a few times if we can, but public API usually doesn't allow offset/limit on search/venue easily 
            // depending on docs (which are sparse/deprecated).
            // Actually, Untappd V4 often uses `offset` or `limit`.

            int offset = 0;
            int limit = 50;
            int totalFetched = 0;
            int maxToFetch = 100; // Let's limit to 100 to be safe and "not aggressive"

            string BaseUrl = "https://api.untappd.com/v4/";

            while (totalFetched < maxToFetch)
            {
                string url = $"{BaseUrl}search/venue?q={query}&client_id={ClientId}&client_secret={ClientSecret}&limit={limit}&offset={offset}";

                Console.WriteLine($"Fetching from: {url}..."); // Be careful not to log secrets in production logs, but this is local dev.

                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(errorContent);
                    break;
                }

                string json = await response.Content.ReadAsStringAsync();
                // Debug: write json to file snippet
                await System.IO.File.WriteAllTextAsync("debug_response.json", json);

                UntappdSearchVenueRoot root = null;
                try
                {
                    root = JsonConvert.DeserializeObject<UntappdSearchVenueRoot>(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Deserialization failed: {ex.Message}");
                    Console.WriteLine($"JSON snippet: {json.Substring(0, Math.Min(json.Length, 500))}");
                    break;
                }

                if (root?.Response?.Venues?.Items == null || root.Response.Venues.Items.Count == 0)
                {
                    Console.WriteLine("No more venues found.");
                    break;
                }

                Console.WriteLine($"Fetched {root.Response.Venues.Items.Count} venues.");

                foreach (var item in root.Response.Venues.Items)
                {
                    var v = item.Venue;
                    if (v == null) continue;

                    // Filter for Australia
                    if (!v.Location.Contains("Australia") && !v.Location.Contains("VIC"))
                    {
                        // Console.WriteLine($"Skipping {v.Venue_name} ({v.Location})");
                        continue;
                    }

                    // Filter by Category
                    if (v.Categories?.Items != null)
                    {
                        var excludedCategories = new HashSet<string> { "Plane", "Airport", "Office", "Home (private)", "Travel & Transport", "Travel and Transportation" };
                        if (v.Categories.Items.Any(c => excludedCategories.Contains(c.CategoryName)))
                        {
                            Console.WriteLine($"[Skip Category]: {v.Venue_name} ({v.Categories.Items.First().CategoryName})");
                            continue;
                        }
                    }

                    // Check if venue exists
                    var existing = await context.Establishments.FindAsync(v.Venue_id);

                    // Fetch Venue Details for Lat/Long and freshness
                    // We need this because search result doesn't have lat/long (it has location string)
                    Console.WriteLine($"Fetching details for {v.Venue_name} ({v.Venue_id})...");
                    string detailUrl = $"{BaseUrl}venue/info/{v.Venue_id}?client_id={ClientId}&client_secret={ClientSecret}";
                    var detailResponse = await httpClient.GetAsync(detailUrl);
                    if (!detailResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"[Error] Failed to get details: {detailResponse.StatusCode}");
                        continue;
                    }

                    string detailJson = await detailResponse.Content.ReadAsStringAsync();
                    UntappdVenueInfoRoot detailRoot = null;
                    try
                    {
                        detailRoot = JsonConvert.DeserializeObject<UntappdVenueInfoRoot>(detailJson);
                    }
                    catch
                    {
                        Console.WriteLine("[Error] Failed to deserialize details.");
                        continue;
                    }

                    var detailVenue = detailRoot?.Response?.Venue;
                    if (detailVenue == null) continue;

                    if (detailVenue.IsClosed)
                    {
                        Console.WriteLine($"[Skip Closed]: {detailVenue.VenueName}");
                        continue;
                    }

                    // Update existing or create new
                    if (existing == null)
                    {
                        var est = new Establishment
                        {
                            EstablishmentId = detailVenue.VenueId,
                            EstablishmentName = detailVenue.VenueName,
                            Category = detailVenue.Categories?.Items?.FirstOrDefault()?.CategoryName,
                            Lat = detailVenue.Location?.Lat ?? 0,
                            Long = detailVenue.Location?.Lng ?? 0,
                            LocationId = melbourneLocation.LocationId,
                            BaseZoom = 10,
                            MaxedCheckinHistory = false
                        };
                        // Update Melb Location details if missing
                        if (string.IsNullOrEmpty(melbourneLocation.State) && !string.IsNullOrEmpty(detailVenue.Location?.VenueState))
                        {
                            melbourneLocation.State = detailVenue.Location.VenueState;
                            melbourneLocation.Country = detailVenue.Location.VenueCountry;
                        }

                        context.Establishments.Add(est);
                        Console.WriteLine($"[+]: {est.EstablishmentName} ({est.Category})");
                    }
                    else
                    {
                        bool updated = false;
                        // Update Lat/Long if missing or changed
                        if (Math.Abs(existing.Lat - (detailVenue.Location?.Lat ?? 0)) > 0.0001 ||
                            Math.Abs(existing.Long - (detailVenue.Location?.Lng ?? 0)) > 0.0001)
                        {
                            existing.Lat = detailVenue.Location?.Lat ?? 0;
                            existing.Long = detailVenue.Location?.Lng ?? 0;
                            updated = true;
                        }

                        // Backfill Category
                        if (string.IsNullOrEmpty(existing.Category) && detailVenue.Categories?.Items?.Any() == true)
                        {
                            existing.Category = detailVenue.Categories.Items.First().CategoryName;
                            updated = true;
                        }

                        if (updated)
                            Console.WriteLine($"[U]: {existing.EstablishmentName} updated.");
                        else
                            Console.WriteLine($"[=]: {existing.EstablishmentName} exists.");
                    }

                    // Rate limit for details call
                    await Task.Delay(2000);
                }

                await context.SaveChangesAsync();

                totalFetched += root.Response.Venues.Items.Count;
                offset += root.Response.Venues.Items.Count;

                Console.WriteLine($"Total processed: {totalFetched}");

                // Buffer sleep
                Console.WriteLine("Sleeping 1s...");
                await Task.Delay(1000);
            }

            Console.WriteLine("Done.");
        }

        // --- Search Response Classes ---
        public class UntappdSearchVenueRoot
        {
            public UntappdSearchVenueResponse Response { get; set; }
        }

        public class UntappdSearchVenueResponse
        {
            public UntappdSearchVenueList Venues { get; set; }
        }

        public class UntappdSearchVenueList
        {
            public int Count { get; set; }
            public List<UntappdSearchVenueItem> Items { get; set; }
        }

        public class UntappdSearchVenueItem
        {
            public UntappdVenue Venue { get; set; }
        }

        // We can reuse UntappdVenue from UntappdResponse.cs if it matches, 
        // but based on typical Untappd JSON, let's include a partial compatible class here to be safe
        // or rely on Newtonsoft to map to existing classes if we reference them.
        // However, the JSON shows "location" object inside venue.
        // Let's define it locally to ensure it matches the SEARCH response structure specifically.

        public class UntappdVenue
        {
            [JsonProperty("venue_id")]
            public int Venue_id { get; set; }

            [JsonProperty("venue_name")]
            public string Venue_name { get; set; }

            [JsonProperty("location")]
            public string Location { get; set; }

            // Lat/Lng NOT in search response for public API

            [JsonProperty("categories")]
            public UntappdVenueCategories Categories { get; set; }
        }

        public class UntappdVenueCategories
        {
            [JsonProperty("items")]
            public List<UntappdVenueCategoryItem> Items { get; set; }
        }

        public class UntappdVenueCategoryItem
        {
            [JsonProperty("category_name")]
            public string CategoryName { get; set; }
        }

        // --- Venue Info Response Classes ---
        public class UntappdVenueInfoRoot
        {
            [JsonProperty("response")]
            public UntappdVenueInfoResponse Response { get; set; }
        }

        public class UntappdVenueInfoResponse
        {
            [JsonProperty("venue")]
            public UntappdVenueDetail Venue { get; set; }
        }

        public class UntappdVenueDetail
        {
            [JsonProperty("venue_id")]
            public int VenueId { get; set; }

            [JsonProperty("venue_name")]
            public string VenueName { get; set; }

            [JsonProperty("is_closed")]
            public bool IsClosed { get; set; }

            [JsonProperty("categories")]
            public UntappdVenueCategories Categories { get; set; }

            [JsonProperty("location")]
            public UntappdVenueDetailLocation Location { get; set; }
        }

        public class UntappdVenueDetailLocation
        {
            [JsonProperty("lat")]
            public double Lat { get; set; }

            [JsonProperty("lng")]
            public double Lng { get; set; }

            [JsonProperty("venue_state")]
            public string VenueState { get; set; }

            [JsonProperty("venue_country")]
            public string VenueCountry { get; set; }
        }
    }
}
