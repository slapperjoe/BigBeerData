using System.Collections.Generic;
using System.Net;
using BigBeerData.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App2.Locations
{
    public class LocationStyles
    {
        private readonly ILogger _logger;

        private readonly BigBeerContext _context;

        public LocationStyles(BigBeerContext context, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LocationStyle>();
            _context = context;
        }

        [Function("LocationStyles")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Location/{id}/style")] HttpRequestData req, long id)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            
            var resultGroup = new List<LocationStyle>();

            var timeCutoff = DateTime.Now.AddDays(-30);
            var resultSet = await _context.Checkins
                .Where(a => a.Establishment.LocationId == id && a.CheckinTime >= timeCutoff)
                .Select(b => new Tuple<Establishment, Beer>(b.Establishment, b.Beer)).ToListAsync();
            var resultEstab = resultSet.GroupBy(b => b.Item1);
            if (resultEstab.Any())
            {
                var countMax = resultEstab.Max(a => a.Count());
                resultGroup = resultEstab.Select(a =>
                {
                    var styleCount = 0;
                    return new LocationStyle
                    {
                        Total = a.Count(),
                        Venue = a.Key.EstablishmentId,
                        Name = a.Key.EstablishmentName,
                        Location = new GeoPoint { X = a.Key.Long, Y = a.Key.Lat },
                        Styles = a.GroupBy(g => g.Item2.BaseStyle)
                                        .OrderByDescending(b => b.Count()).Select(s =>
                                        {
                                            var result = new StyleResult { Count = s.Count(), Name = s.Key, Height = styleCount };
                                            styleCount += s.Count();
                                            return result;
                                        })
                            .ToList()
                    };
                }).OrderBy(a => a.Name).ToList();
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(resultGroup);

            return response;
        }
    }
}
