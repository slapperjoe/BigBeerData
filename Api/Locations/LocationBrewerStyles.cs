using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
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
	public class LocationBrewerStyles
	{

		private readonly BigBeerContext _context;
		private readonly ILogger<LocationStyles> _logger;

		public LocationBrewerStyles(BigBeerContext context, ILogger<LocationStyles> log)
		{
			_context = context;
			_logger = log;
		}


		[FunctionName("LocationBrewerStyles")]
		[OpenApiOperation(operationId: "Run", tags: new[] { "id", "style" })]
		[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
		[OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = true, Type = typeof(long), Description = "The **id** parameter")]
		[OpenApiParameter(name: "style", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **style** parameter")]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
		public async Task<IActionResult> Run(
							[HttpTrigger(AuthorizationLevel.Function, "get", Route = "Location/{id}/{style}/brewers")]
			HttpRequest req, long id, string style)
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
			// results.ForEach(r =>
			// {
			// 	r.BeersBrewed = _context.Beers.Where(a => a.BrewerSLUG == r.SLUG)
			// 											.GroupBy(d => d.BaseStyle).Select(d => new StyleResult { Count = d.Count(), Name = d.Key })
			// 													.OrderByDescending(c => c.Count).ToList();

			// });
			return new OkObjectResult(results.OrderByDescending(a => a.BeersBrewed.ToList().Count).ToArray());
		}
	}
}

