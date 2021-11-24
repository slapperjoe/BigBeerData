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
	public class CurrentLocation
	{
		private readonly BigBeerContext _context;
		private readonly ILogger<LocationStyles> _logger;

		public CurrentLocation(BigBeerContext context, ILogger<LocationStyles> log)
		{
			_context = context;
			_logger = log;
		}

		[FunctionName("CurrentLocation")]
		[OpenApiOperation(operationId: "Run")]
		[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
		public async Task<IActionResult> Run(
						[HttpTrigger(AuthorizationLevel.Function, "get", Route = "location/current")]
			HttpRequest req)
		{
			var establishments = _context.Locations.Where(l => l.Establishments.Any());

			var result = establishments.Select(s =>
				new AreaLocation
				{
					ID = s.LocationId,
					Name = s.LocationName
				});

			return new OkObjectResult(await result.ToListAsync());
		}
	}
}

