using System.Collections.Generic;
using System.Net;
using System.Web;
using BigBeerData.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App2.Locations
{
    public class LocationBrewerStyles
    {
        private readonly ILogger _logger;

        private readonly BigBeerContext _context;

        public LocationBrewerStyles(BigBeerContext context, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LocationBrewerStyles>();

            _context = context;
        }

        [Function("LocationBrewerStyles")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "Location/{id}/{style}/brewers")] HttpRequestData req, long id, string style)
        {
            

            style = HttpUtility.UrlDecode(style);
            _logger.LogDebug("Location Brewer Styles called id: {id}, style: {style}", id, style);

            var timeCutoff = DateTime.Now.AddDays(-30);
            var checkinSet = _context.Checkins
                .Where(a => a.Establishment.EstablishmentId == id && a.CheckinTime >= timeCutoff && a.Beer.BaseStyle == style);

            var resultSet = await checkinSet.Include(a => a.Beer.Brewer.Beers)
                    .Select(a => a.Beer.Brewer).ToListAsync();

            var results = resultSet.Distinct().Select(b => new BrewerResult
            {
                //Id = b.BrewerId,
                Name = b.BrewerName,
                Type = b.Type,
                URL = b.URL,
                Location = new GeoPoint { X = b.Long, Y = b.Lat },
                BeersBrewed = b.Beers.GroupBy(d => d.BaseStyle).Select(d => new StyleResult { Count = d.Count(), Name = d.Key })
            }).ToList();


            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(results.OrderByDescending(a => a.BeersBrewed.ToList().Count).ToArray());

            return response;
        }
    }
}
