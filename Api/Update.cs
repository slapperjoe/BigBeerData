using System.Diagnostics;
using System.Net;
using System.Text.Json;
using BigBeerData.Shared;
using BigBeerData.Shared.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api
{
    public class Update
    {
        private static readonly ActivitySource ActivitySource = new("Api.Update");
        private readonly ILogger<Update> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string client_id;
        private readonly string client_secret;
        const int MAX_REQUESTS = 100;

        private readonly string _connectionString;

        public Update(BigBeerContext context, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
        {
            client_id = System.Environment.GetEnvironmentVariable("client_id") ?? String.Empty;
            client_secret = System.Environment.GetEnvironmentVariable("client_secret") ?? String.Empty;
            _logger = loggerFactory.CreateLogger<Update>();
            _httpClientFactory = httpClientFactory;
            _connectionString = context.Database.GetDbConnection().ConnectionString;
        }

        [Function("Update")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
        {
            using var activity = ActivitySource.StartActivity("Update.Run");
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8"); // Changed from text/event-stream for browser visibility
            response.Headers.Add("Cache-Control", "no-cache");

            try
            {
                await RunUpdate(response.Body, req.FunctionContext.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update Failure");
                using var writer = new StreamWriter(response.Body, leaveOpen: true);
                await writer.WriteLineAsync($"ERROR: {ex}");
                await writer.FlushAsync();
            }
            return response;
        }

        [Function("UpdateTimer")]
        public async Task RunTimer([TimerTrigger("0 0 0 * * 0")] TimerInfo myTimer, FunctionContext context)
        {
            _logger.LogInformation($"Weekly Update Timer executed at: {DateTime.Now}");
            if (myTimer.IsPastDue) _logger.LogInformation("Timer is running late!");

            // Timer runs with full limit (250) or config based. Let's stick to 250.
            var requestMonitor = new RequestMonitor(250);
            await RunUpdateCore(async (msg) => _logger.LogInformation(msg), requestMonitor, context.CancellationToken);
        }

        public async Task RunUpdate(Stream stream, CancellationToken cancellationToken)
        {
            using var writeStream = new StreamWriter(stream, leaveOpen: true) { AutoFlush = true };
            await writeStream.WriteLineAsync("Starting Data Scrape (Total Limit 250)...");
            await writeStream.FlushAsync();

            var requestMonitor = new RequestMonitor(250);

            await RunUpdateCore(async (msg) =>
            {
                await writeStream.WriteLineAsync($"{msg}");
                await writeStream.FlushAsync();
            }, requestMonitor, cancellationToken);

            await writeStream.WriteLineAsync("Update complete.");
            await writeStream.FlushAsync();
        }

        private async Task RunUpdateCore(Func<string, Task> logAction, RequestMonitor monitor, CancellationToken cancellationToken)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BigBeerContext>();
            optionsBuilder.UseSqlServer(_connectionString);

            using var dbContext = new BigBeerContext(optionsBuilder.Options);
            var client = _httpClientFactory.CreateClient("BeerBot");

            List<Establishment> establishments = dbContext.Establishments.Include(i => i.Checkins).ToList();

            foreach (var establishment in establishments)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await HandleEstablishment(establishment, dbContext, client, logAction, monitor, cancellationToken);
            }
        }

        private async Task HandleEstablishment(Establishment establishment, BigBeerContext dbContext, HttpClient client, Func<string, Task> logAction, RequestMonitor monitor, CancellationToken cancellationToken)
        {
            var counter = 0;
            var alreadyAddedToDatabase = false;
            var newGet = !establishment.Checkins.Any();
            List<Checkin>? checkins = null;
            var updateTime = DateTime.UtcNow;

            if (!establishment.LastCheckinUpdate.HasValue || establishment.LastCheckinUpdate.Value.Date < updateTime.Date)
            {
                establishment.LastCheckinUpdate = updateTime;
                dbContext.Establishments.Update(establishment);
                await dbContext.SaveChangesAsync(cancellationToken);

                await logAction($"Looking for new beers in {establishment.EstablishmentName}");
                checkins = await CheckinsGet(establishment.EstablishmentId, client, logAction, monitor);
            }
            else
            {
                await logAction($"Already searched today for beers in {establishment.EstablishmentName}");
            }

            counter++;

            if (checkins != null)
            {
                alreadyAddedToDatabase = await ProcessCheckins(checkins, alreadyAddedToDatabase, dbContext, logAction, cancellationToken);
                while (!alreadyAddedToDatabase && counter < MAX_REQUESTS)
                {
                    await logAction($"Looking for less new beers in {establishment.EstablishmentName}");

                    checkins = await CheckinsGet(establishment.EstablishmentId, client, logAction, monitor);
                    counter++;
                    alreadyAddedToDatabase = await ProcessCheckins(checkins, alreadyAddedToDatabase, dbContext, logAction, cancellationToken);
                }
            }

            if (counter < MAX_REQUESTS && !newGet && !establishment.MaxedCheckinHistory)
            {
                checkins = await CheckinsGet(establishment.EstablishmentId, client, logAction, monitor);
                counter++;
                alreadyAddedToDatabase = await ProcessCheckins(checkins, alreadyAddedToDatabase, dbContext, logAction, cancellationToken);

                while (checkins.Count > 0 && !alreadyAddedToDatabase && counter < MAX_REQUESTS)
                {
                    await logAction($"Looking for old beers in {establishment.EstablishmentName}");
                    checkins = await CheckinsGet(establishment.EstablishmentId, client, logAction, monitor);
                    counter++;
                    alreadyAddedToDatabase = await ProcessCheckins(checkins, alreadyAddedToDatabase, dbContext, logAction, cancellationToken);
                }
            }
        }

        private static async Task<bool> ProcessCheckins(List<Checkin> checkins, bool alreadyAdded, BigBeerContext db, Func<string, Task> logAction, CancellationToken cancellationToken)
        {
            foreach (var checkin in checkins)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (checkin.Beer == null)
                {
                    await logAction("Skipped checkin missing beer payload");
                    continue;
                }

                if (checkin.Beer.Brewer != null)
                {
                    var previousBrewer = db.Brewers.Local.FirstOrDefault(b => b.BrewerId == checkin.Beer.Brewer.BrewerId)
                                         ?? db.Brewers.FirstOrDefault(b => b.BrewerId == checkin.Beer.Brewer.BrewerId);
                    if (previousBrewer != null)
                    {
                        checkin.Beer.Brewer = previousBrewer;
                    }
                }
                else
                {
                    var prevBeer = db.Beers.Local.FirstOrDefault(b => b.Bid == checkin.Beer.Bid)
                                   ?? db.Beers.FirstOrDefault(b => b.Bid == checkin.Beer.Bid);
                    if (prevBeer != null)
                        checkin.Beer = prevBeer;
                }

                var existingBeer = db.Beers.Local.FirstOrDefault(b => b.Bid == checkin.Beer.Bid)
                                   ?? db.Beers.FirstOrDefault(b => b.Bid == checkin.Beer.Bid);
                if (existingBeer != null)
                {
                    checkin.Beer = existingBeer;
                }

                var duplicate = db.Checkins.Any(b => b.CheckinTime == checkin.CheckinTime || b.CheckinId == checkin.CheckinId);
                if (duplicate)
                {
                    alreadyAdded = true;
                    continue;
                }

                try
                {
                    db.Checkins.Add(checkin);
                    await logAction(" + Added " + checkin?.Beer?.BeerName);
                }
                catch (Exception ex)
                {
                    await logAction($"Failed to add checkin: {checkin.CheckinId},{ex.Message}");
                }
            }

            await db.SaveChangesAsync(cancellationToken);
            return alreadyAdded;
        }

        public async Task<List<Checkin>> CheckinsGet(int id, HttpClient client, Func<string, Task> logAction, RequestMonitor monitor, int? max_id = null)
        {
            // Check request limit before call
            monitor.IncrementAndCheck();

            var requestString = "venue/checkins/" + id + "?client_id=" + client_id +
                            "&client_secret=" + client_secret;
            if (max_id.HasValue)
            {
                requestString += "&max_id=" + max_id.Value;
            }

            var venueResult = await client.GetAsync(requestString);

            if (venueResult.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new Exception("Rate Limit Exceeded (429). Halting Update.");
            }

            var venueData = await venueResult.Content.ReadAsStringAsync();

            var serializerOptions = new JsonSerializerOptions
            {
                Converters = { new DynamicJsonConverter() }
            };
            dynamic? payload = JsonSerializer.Deserialize<dynamic>(venueData, serializerOptions);
            if (payload?.response == null)
            {
                await logAction("Venue has no items");
                return new List<Checkin>();
            }

            var checkinItems = (IEnumerable<dynamic>)payload.response.checkins.items;
            if (checkinItems == null)
            {
                return new List<Checkin>();
            }

            var checkinSet = checkinItems.Select(item => new Checkin
            {
                CheckinId = (int)item.checkin_id,
                CheckinTime = DateTime.Parse(item.created_at),
                Beer = GetBeer(item),
                Rating = item.rating_score,
                EstablishmentId = id
            });

            return checkinSet.ToList();
        }

        private static Beer GetBeer(dynamic a)
        {
            var beer = new Beer
            {
                Bid = (int)a.beer.bid,
                ABV = a.beer.beer_abv,
                BeerName = a.beer.beer_name,
                BeerPic = a.beer.beer_label,
                SLUG = a.beer.beer_slug,
                Style = a.beer.beer_style,
                Brewer = GetBrewer(a.brewery)
            };
            return beer;
        }

        private static Brewer GetBrewer(dynamic b)
        {
            var brewer = new Brewer
            {
                BrewerId = (int)b.brewery_id,
                SLUG = b.brewery_slug,
                BrewerName = b.brewery_name,
                City = b.location.brewery_city,
                State = b.location.brewery_state,
                Country = b.country_name,
                Lat = b.location.lat,
                Long = b.location.lng,
                Type = b.brewery_type,
                URL = b.contact.url
            };
            return brewer;
        }
    }

    public class RequestMonitor
    {
        private int _count = 0;
        private readonly int _limit;

        public RequestMonitor(int limit)
        {
            _limit = limit;
        }

        public void IncrementAndCheck()
        {
            _count++;
            if (_count > _limit)
            {
                throw new Exception($"Total Request Limit ({_limit}) Exceeded. Halting.");
            }
        }
    }
}
