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
			response.Headers.Add("Content-Type", "text/event-stream");
			response.Headers.Add("Cache-Control", "no-cache");

			await RunUpdate(response.Body, req.FunctionContext.CancellationToken);
			return response;
		}

		[Function("UpdateSync")]
		public async Task<HttpResponseData> RunSync(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req)
		{
			using var activity = ActivitySource.StartActivity("Update.RunSync");
			_logger.LogInformation("C# HTTP trigger function processed a request.");

			var response = req.CreateResponse(HttpStatusCode.OK);
			response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

			await RunUpdate(response.Body, req.FunctionContext.CancellationToken);
			return response;
		}



		public async Task RunUpdate(Stream stream, CancellationToken cancellationToken)
		{
			var optionsBuilder = new DbContextOptionsBuilder<BigBeerContext>();
			optionsBuilder.UseSqlServer(_connectionString);

			using var dbContext = new BigBeerContext(optionsBuilder.Options);
			using var writeStream = new StreamWriter(stream, leaveOpen: true) { AutoFlush = true };
			var client = _httpClientFactory.CreateClient("BeerBot");

			await writeStream.WriteLineAsync("data: Starting Data Scrape.");
			await writeStream.FlushAsync();

			List<Establishment> establishments = dbContext.Establishments.Include(i => i.Checkins).ToList();

			foreach (var establishment in establishments)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await HandleEstablishment(establishment, dbContext, client, writeStream, cancellationToken);
			}

			await writeStream.WriteLineAsync("data: Update complete.");
			await writeStream.FlushAsync();
		}

		private async Task HandleEstablishment(Establishment establishment, BigBeerContext dbContext, HttpClient client, StreamWriter writeStream, CancellationToken cancellationToken)
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

				await writeStream.WriteLineAsync($"data: Looking for new beers in {establishment.EstablishmentName}");
				await writeStream.FlushAsync();
				checkins = await CheckinsGet(establishment.EstablishmentId, client, writeStream);
			}
			else
			{
				await writeStream.WriteLineAsync($"data: Already searched today for beers in {establishment.EstablishmentName}");
			}

			counter++;

			if (checkins != null)
			{
				alreadyAddedToDatabase = await ProcessCheckins(checkins, alreadyAddedToDatabase, dbContext, writeStream, cancellationToken);
				while (!alreadyAddedToDatabase && counter < MAX_REQUESTS)
				{
					await writeStream.WriteLineAsync($"data: Looking for less new beers in {establishment.EstablishmentName}");
					await writeStream.FlushAsync();

					checkins = await CheckinsGet(establishment.EstablishmentId, client, writeStream);
					counter++;
					alreadyAddedToDatabase = await ProcessCheckins(checkins, alreadyAddedToDatabase, dbContext, writeStream, cancellationToken);
				}
			}

			if (counter < MAX_REQUESTS && !newGet && !establishment.MaxedCheckinHistory)
			{
				checkins = await CheckinsGet(establishment.EstablishmentId, client, writeStream);
				counter++;
				alreadyAddedToDatabase = await ProcessCheckins(checkins, alreadyAddedToDatabase, dbContext, writeStream, cancellationToken);

				while (checkins.Count > 0 && !alreadyAddedToDatabase && counter < MAX_REQUESTS)
				{
					await writeStream.WriteLineAsync("data: Looking for old beers in {establishment.EstablishmentName}");
					await writeStream.FlushAsync();
					checkins = await CheckinsGet(establishment.EstablishmentId, client, writeStream);
					counter++;
					alreadyAddedToDatabase = await ProcessCheckins(checkins, alreadyAddedToDatabase, dbContext, writeStream, cancellationToken);
				}
			}
		}

		private static async Task<bool> ProcessCheckins(List<Checkin> checkins, bool alreadyAdded, BigBeerContext db, StreamWriter sw, CancellationToken cancellationToken)
		{
			foreach (var checkin in checkins)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var existingBeer = db.Beers.FirstOrDefault(b => b.Bid == checkin.Beer.Bid);
				if (existingBeer != null)
				{
					checkin.Beer = existingBeer;
				}

				if (checkin.Beer.Brewer != null)
				{
					var previousBrewer = db.Brewers.FirstOrDefault(b => b.BrewerId == checkin.Beer.Brewer.BrewerId);
					if (previousBrewer != null)
					{
						checkin.Beer.Brewer = previousBrewer;
					}
				}
				else
				{
					var prevBeer = db.Beers.FirstOrDefault(b => b.Bid == checkin.Beer.Bid);
					checkin.Beer = prevBeer;
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
					await sw.WriteLineAsync(" + Added " + checkin?.Beer?.BeerName);
				}
				catch (Exception ex)
				{
					await sw.WriteLineAsync($"Failed to add checkin: {checkin.CheckinId},{ex.Message}");
				}
			}

			await db.SaveChangesAsync(cancellationToken);
			return alreadyAdded;
		}

		public async Task<List<Checkin>> CheckinsGet(int id, HttpClient client, StreamWriter sw, int? max_id = null)
		{
			var requestString = "venue/checkins/" + id + "?client_id=" + client_id +
							"&client_secret=" + client_secret;
			if (max_id.HasValue)
			{
				requestString += "&max_id=" + max_id.Value;
			}

			var venueResult = await client.GetAsync(requestString);

			var venueData = await venueResult.Content.ReadAsStringAsync();

			var serializerOptions = new JsonSerializerOptions
			{
				Converters = { new DynamicJsonConverter() }
			};
			dynamic? payload = JsonSerializer.Deserialize<dynamic>(venueData, serializerOptions);
			if (payload?.response == null)
			{
				await sw.WriteLineAsync("Venue has no items");
				return new List<Checkin>();
			}

			var checkinItems = (IEnumerable<dynamic>)payload.response.checkins.items;

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
}
