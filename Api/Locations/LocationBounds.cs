using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BigBeerData.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace Api.Locations
{
	public class LocationBounds
	{
		private readonly BigBeerContext _context;
		private readonly ILogger<LocationStyles> _logger;

		public LocationBounds(BigBeerContext context, ILogger<LocationStyles> log)
		{
			_context = context;
			_logger = log;
		}

		[FunctionName("LocationBounds")]
		[OpenApiOperation(operationId: "Run", tags: new[] { "id" })]
		[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
		[OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = true, Type = typeof(long), Description = "The **id** parameter")]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
		public async Task<IActionResult> Run(
						[HttpTrigger(AuthorizationLevel.Function, "get"  , Route = "Geo/bounds/{id}")]
			HttpRequest req,	long id)
		{
			var locations = await _context.Establishments.Where(a => a.LocationId == id).ToListAsync();
			return new OkObjectResult(new GeoBounds(locations));
		}
	}
}

