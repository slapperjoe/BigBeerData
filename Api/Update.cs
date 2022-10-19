using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using BigBeerData.Shared;
using BigBeerData.Shared.Utils;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api
{
	public class Update
	{
		private readonly ILogger<Update> _logger;
		private readonly IHttpClientFactory _httpClientFactory;
		const string client_id = "046C4B60EFFB73F16B087B2319EAE8CBB5F44845";
		const string client_secret = "3308A2DCF939C0B5E50EDB1B5CAE74C61BB82D12";
		const int MAX_REQUESTS = 100;
		private readonly BigBeerContext _context;

		private readonly string _connectionString;

		public Update(BigBeerContext context, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
		{
			_logger = loggerFactory.CreateLogger<Update>();
			_httpClientFactory = httpClientFactory;
			_context = context;
			_connectionString = _context.Database.GetDbConnection().ConnectionString;
		}

		[Function("Update")]
		public HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestData req)
		{
			_logger.LogInformation("C# HTTP trigger function processed a request.");

			var features = req.FunctionContext.Features.Get<IHttpBodyControlFeature>();
			if (features != null)
			{
				features.AllowSynchronousIO = true;
			}
			var response = new HttpResponseMessage
			{
				Content = new PushStreamContent(new Func<Stream, HttpContent, TransportContext, Task>(RunUpdate), "text/event-stream")
			};
			response.Headers.TransferEncodingChunked = true;

			return response;
		}

		public async Task RunUpdate(Stream stream, HttpContent content, TransportContext transContext)
		{
			var optionsBuilder = new DbContextOptionsBuilder<BigBeerContext>();
			optionsBuilder.UseSqlServer(_connectionString);

			using (var dbContext = new BigBeerContext(optionsBuilder.Options))
			{
				var writeStream = new StreamWriter(stream) { AutoFlush = true };
				var client = _httpClientFactory.CreateClient("BeerBot");

				List<Checkin>? t;
				var counter = 0;
				var alreadyAddedToDatabase = false;
				var newGet = false;

				await writeStream.WriteLineAsync("data: Starting Data Scrape.");
				List<Establishment> establishments = dbContext.Establishments.Include(i => i.Checkins).ToList();

				foreach (var establishment in establishments)
				{
					t = null;

					newGet = !establishment.Checkins.Any();

					var updateTime = DateTime.Now;

					if (!establishment.LastCheckinUpdate.HasValue ||
													establishment.LastCheckinUpdate.Value.Date < updateTime.Date)
					{
						establishment.LastCheckinUpdate = updateTime;
						dbContext.Establishments.Update(establishment);
						dbContext.SaveChanges();

						//get newest	
						await writeStream.WriteLineAsync($"Looking for new beers in {establishment.EstablishmentName}");
						t = await CheckinsGet(establishment.EstablishmentId, client, writeStream);
					}
					else
					{
						await writeStream.WriteLineAsync($"Already searched today for beers in {establishment.EstablishmentName}");
					}

					counter++;

					try
					{
						if (t != null)
						{
							//process and keep getting new unstored
							alreadyAddedToDatabase = await ProcessCheckins(t, alreadyAddedToDatabase, dbContext, _logger, writeStream);
							while (!alreadyAddedToDatabase && counter < MAX_REQUESTS)
							{
								var newMax = t.OrderByDescending(a => a.CheckinTime).Last().CheckinTime;

								await writeStream.WriteLineAsync($"Looking for less new beers in {establishment.EstablishmentName}");

								t = await CheckinsGet(establishment.EstablishmentId, client, writeStream);
								counter++;
								try
								{
									alreadyAddedToDatabase = await ProcessCheckins(t, alreadyAddedToDatabase, dbContext, _logger, writeStream);
								}
								catch (Exception e)
								{
									UpdateEstablishment(establishment, e);
									dbContext.Establishments.Update(establishment);
									dbContext.SaveChanges();
									await writeStream.WriteLineAsync($"Updating Establishment {e}");
									counter = MAX_REQUESTS;
								}
							}
						}

						//get older
						if (counter < MAX_REQUESTS && !newGet && !establishment.MaxedCheckinHistory)
						{
							t = await CheckinsGet(establishment.EstablishmentId, client, writeStream);
							counter++;
							try
							{
								alreadyAddedToDatabase = await ProcessCheckins(t, alreadyAddedToDatabase, dbContext, _logger, writeStream);
								while (t.Count > 0 && !alreadyAddedToDatabase && counter < MAX_REQUESTS)
								{
									var newMax = t.OrderByDescending(a => a.CheckinTime).Last().CheckinTime;

									await writeStream.WriteLineAsync("Looking for old beers in {establishment.EstablishmentName}");
									t = await CheckinsGet(establishment.EstablishmentId, client, writeStream);
									counter++;
									try
									{
										alreadyAddedToDatabase = await ProcessCheckins(t, alreadyAddedToDatabase, dbContext, _logger, writeStream);
									}
									catch (Exception e)
									{
										UpdateEstablishment(establishment, e);
										dbContext.Establishments.Update(establishment);
										dbContext.SaveChanges();
										await writeStream.WriteLineAsync($"Updating Establishment {e}");
										counter = MAX_REQUESTS;
									}
								}
							}
							catch (Exception e)
							{
								UpdateEstablishment(establishment, e);
								dbContext.Establishments.Update(establishment);
								dbContext.SaveChanges();
								await writeStream.WriteLineAsync($"Failed to update {e}");
							}
						}
					}
					catch (Exception e)
					{
						UpdateEstablishment(establishment, e);
						await writeStream.WriteLineAsync($"Failed to update {e}");
					}

				}
				await writeStream.WriteLineAsync("Update complete.");

				await writeStream.FlushAsync();
				stream.Close();
			}
		}

		private static void UpdateEstablishment(Establishment establishment, Exception e)
		{
			if (e.Message.IndexOf("Your 'max_id' is too low,") > 0)
			{
				establishment.MaxedCheckinHistory = true;
			}
		}

		private async static Task<bool> ProcessCheckins(List<Checkin> t, bool alreadyAdded, BigBeerContext db, ILogger log, StreamWriter sw)
		{
			await Task.Run(() =>
			{
				t.ForEach(async a =>
				{
					var previousBeer = db.Beers.FirstOrDefault(b => b.Bid == a.Beer.Bid);
					if (previousBeer != null)
					{
						a.Beer = previousBeer;
					}

					if (a.Beer.Brewer != null)
					{
						var previousBrewer = db.Brewers.FirstOrDefault(b => b.BrewerId == a.Beer.Brewer.BrewerId);
						if (previousBrewer != null)
						{
							a.Beer.Brewer = previousBrewer;
						}
					}
					else
					{
						var prevBeer = db.Beers.FirstOrDefault(b => b.Bid == a.Beer.Bid);
						a.Beer = prevBeer;
					}

					if (db.Checkins.FirstOrDefault(b => b.CheckinTime == a.CheckinTime) != null ||
																										db.Checkins.Any(b => b.CheckinId == a.CheckinId))
					{
						alreadyAdded = true;
					}
					else
					{
						try
						{
							db.Checkins.Add(a);
							await sw.WriteLineAsync(" + Added " + a?.Beer?.BeerName);
						}
						catch (Exception ex)
						{
							await sw.WriteLineAsync($"Failed to add checkin: {a.CheckinId},{ex}");
						}
					}

					db.SaveChanges();

				});
			});
			return alreadyAdded;
		}

		public async static Task<List<Checkin>> CheckinsGet(int id, HttpClient client, StreamWriter sw, int? max_id = null)
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
			dynamic? stuff = JsonSerializer.Deserialize<dynamic>(venueData, serializerOptions);

			// if (stuff.meta.code >= 400 && stuff.meta.code < 500)
			// {
			// 	string errorString = stuff.meta.error_detail.ToString();
			// 	throw new Exception(errorString);
			// }
			try
			{
				if (stuff?.response == null)
				{
					await sw.WriteLineAsync("Venue has no items");
				}
				else
				{
					var checkinItems = (IEnumerable<dynamic>)stuff.response.checkins.items;

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
				return new List<Checkin>();

			}
			catch (Exception)
			{
				throw;
			}
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
