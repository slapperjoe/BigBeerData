using System.Collections.Generic;
using System.Net;
using BigBeerData.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App2.Locations
{
    public class LocationBounds
    {
        private readonly ILogger _logger;
        private readonly BigBeerContext _context;

        public LocationBounds(BigBeerContext context, ILoggerFactory loggerFactory)
        {

            _logger = loggerFactory.CreateLogger<LocationBounds>();
        }

        [Function("LocationBounds")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get",Route = "Geo/bounds/{id}")] HttpRequestData req, long id)
        {
            _logger.LogInformation("Location Bounds called: id: {id}", id);  
            var locations = await _context.Establishments.Where(a => a.LocationId == id).ToListAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(new GeoBounds(locations));

            return response;
        }
    }
}
