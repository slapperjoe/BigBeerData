using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BigBeerData.Shared;
using BigBeerData.Shared.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace BigBeerData.Functions
{

    public class Update
    {
        private readonly ILogger<Update> _logger;
        private readonly IHttpClientFactory httpClientFactory;
        const string client_id = "046C4B60EFFB73F16B087B2319EAE8CBB5F44845";
        const string client_secret = "3308A2DCF939C0B5E50EDB1B5CAE74C61BB82D12";
        const int MAX_REQUESTS = 100;
        private readonly BigBeerContext _context;

        public Update(BigBeerContext context, IHttpClientFactory httpClientFactory, ILogger<Update> log)
        {
            this.httpClientFactory = httpClientFactory;
            _logger = log;
            _context = context;
        }

        [FunctionName("TestStreaming")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "TestStream," })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task Run1(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req)
        {
            var response = req.HttpContext.Response;
            response.StatusCode = 200;
            response.ContentType = "text/plain";
            await using var sw = new StreamWriter(response.Body);
            await foreach (var msg in GetDataAsync())
            {
                await sw.WriteLineAsync(msg);
                await sw.FlushAsync();
                await Task.Delay(1000);
            }


            await sw.FlushAsync();
        }

        private async IAsyncEnumerable<string> GetDataAsync()
        {
            for (var i = 0; i < 100; ++i)
            {
                yield return $"Line {i}";
            }
        }


        [FunctionName("Update")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "UpdateDB" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
                [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req)
        {
            // string name = req.Query["name"];
            // string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var client = httpClientFactory.CreateClient("BeerBot");

            List<Checkin> t;
            var counter = 0;
            var alreadyAddedToDatabase = false;
            var newGet = false;

            var result = new StringBuilder();
            _logger.LogInformation("Starting Data Scrape.");
            List<Establishment> establishments = _context.Establishments.Include(i => i.Checkins).ToList();

            foreach (var establishment in establishments)
            {
                t = null;

                newGet = !establishment.Checkins.Any();

                var updateTime = DateTime.Now;

                if (!establishment.LastCheckinUpdate.HasValue ||
                        establishment.LastCheckinUpdate.Value.Date < updateTime.Date)
                {
                    establishment.LastCheckinUpdate = updateTime;
                    _context.Establishments.Update(establishment);
                    _context.SaveChanges();

                    //get newest	
                    _logger.LogInformation("Looking for new beers in {establishmentName}", establishment.EstablishmentName);
                    t = await CheckinsGet(establishment.EstablishmentId, client);
                }
                else
                {
                    _logger.LogInformation("Already searched today for beers in {establishmentName}", establishment.EstablishmentName);
                }

                counter++;

                try
                {
                    if (t != null)
                    {
                        //process and keep getting new unstored
                        alreadyAddedToDatabase = ProcessCheckins(t, alreadyAddedToDatabase, _context, _logger, result);
                        while (!alreadyAddedToDatabase && counter < MAX_REQUESTS)
                        {
                            var newMax = t.OrderByDescending(a => a.CheckinTime).Last().CheckinTime;

                            _logger.LogInformation("Looking for less new beers in {establishmentName}", establishment.EstablishmentName);

                            t = await CheckinsGet(establishment.EstablishmentId, client);
                            counter++;
                            try
                            {
                                alreadyAddedToDatabase = ProcessCheckins(t, alreadyAddedToDatabase, _context, _logger, result);
                            }
                            catch (Exception e)
                            {
                                UpdateEstablishment(establishment, e);
                                _context.Establishments.Update(establishment);
                                _context.SaveChanges();
                                _logger.LogError(e, "Updating Establishment");
                                counter = MAX_REQUESTS;
                            }
                        }
                    }

                    //get older
                    if (counter < MAX_REQUESTS && !newGet && !establishment.MaxedCheckinHistory)
                    {
                        t = await CheckinsGet(establishment.EstablishmentId, client);
                        counter++;
                        try
                        {
                            alreadyAddedToDatabase = ProcessCheckins(t, alreadyAddedToDatabase, _context, _logger, result);
                            while (!alreadyAddedToDatabase && counter < MAX_REQUESTS)
                            {
                                var newMax = t.OrderByDescending(a => a.CheckinTime).Last().CheckinTime;

                                _logger.LogInformation("Looking for old beers in {establishmentName}", establishment.EstablishmentName);
                                t = await CheckinsGet(establishment.EstablishmentId, client);
                                counter++;
                                try
                                {
                                    alreadyAddedToDatabase = ProcessCheckins(t, alreadyAddedToDatabase, _context, _logger, result);
                                }
                                catch (Exception e)
                                {
                                    UpdateEstablishment(establishment, e);
                                    _context.Establishments.Update(establishment);
                                    _context.SaveChanges();
                                    _logger.LogError(e, "Updating Establishment");
                                    counter = MAX_REQUESTS;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            UpdateEstablishment(establishment, e);
                            _context.Establishments.Update(establishment);
                            _context.SaveChanges();
                            _logger.LogError(e, "Failed to update");


                            return new BadRequestObjectResult(e);
                        }
                    }
                }
                catch (Exception e)
                {
                    UpdateEstablishment(establishment, e);
                    _logger.LogError(e, "Failed to update");

                    return new BadRequestObjectResult(e);
                }

            }
            _logger.LogInformation("Update complete.");

            return new OkObjectResult(result.ToString());
        }

        private static void UpdateEstablishment(Establishment establishment, Exception e)
        {
            if (e.Message.IndexOf("Your 'max_id' is too low,") > 0)
            {
                establishment.MaxedCheckinHistory = true;
            }
        }

        private static bool ProcessCheckins(List<Checkin> t, bool alreadyAdded, BigBeerContext db, ILogger log,
            StringBuilder result)
        {
            t.ForEach(a =>
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

                if (db.Checkins.Any(b => b.CheckinId == a.CheckinId))
                {
                    alreadyAdded = true;
                }
                else
                {
                    db.Checkins.Add(a);
                    result.LogOutput(log, " + Added " + a.Beer.BeerName);
                }

                db.SaveChanges();

            });
            return alreadyAdded;
        }

        public async static Task<List<Checkin>> CheckinsGet(int id, HttpClient client, int? max_id = null)
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
            dynamic stuff = JsonSerializer.Deserialize<dynamic>(venueData, serializerOptions);

            // if (stuff.meta.code >= 400 && stuff.meta.code < 500)
            // {
            // 	string errorString = stuff.meta.error_detail.ToString();
            // 	throw new Exception(errorString);
            // }
            try
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

