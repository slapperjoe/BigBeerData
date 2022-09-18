using System.Collections.Generic;
using System.Net;
using BigBeerData.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Locations
{
    public class CurrentLocation
    {

        private readonly BigBeerContext _context;
        private readonly ILogger _logger;

        public CurrentLocation(BigBeerContext context, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LocationStyle>();
            _context = context;
        }

        [Function("CurrentLocation")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "location/current")] 
        HttpRequestData req)
        {
            var establishments = _context.Locations.Where(l => l.Establishments.Any());

            var result = await establishments.Select(s =>
                new AreaLocation
                {
                    ID = s.LocationId,
                    Name = s.LocationName
                }).ToListAsync();

            _logger.LogInformation("Get Current Location Called.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);

            return response;
        }
    }
}
