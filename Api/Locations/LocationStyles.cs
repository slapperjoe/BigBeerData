using System;
using System.Collections.Generic;
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
	public class LocationStyles
	{
		private readonly BigBeerContext _context;
		private readonly ILogger<LocationStyles> _logger;

		public LocationStyles(BigBeerContext context, ILogger<LocationStyles> log)
		{
			_context = context;
			_logger = log;
		}

		[FunctionName("LocationStyles")]
		[OpenApiOperation(operationId: "Run", tags: new[] { "id" })]
		[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
		[OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **id** parameter")]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
		public async Task<IActionResult> Run(
				[HttpTrigger(AuthorizationLevel.Function, "get", Route = "Location/{id}/style")]
		HttpRequest req, long id)
		{
			
			var resultGroup = new List<LocationStyle>();

			var timeCutoff = DateTime.Now.AddDays(-30);
			var resultSet = await _context.Checkins
				.Where(a => a.Establishment.LocationId == id && a.CheckinTime >= timeCutoff)
				.Select(b => new Tuple<Establishment, Beer>(b.Establishment, b.Beer)).ToListAsync();
			var resultEstab = resultSet
				.GroupBy(b => b.Item1);
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

			return new OkObjectResult(resultGroup);
		}
	}
}

