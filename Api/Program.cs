using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Net.Http.Headers;

using BigBeerData.Shared;

[assembly: FunctionsStartup(typeof(Api.Startup))]
namespace Api
{
	public class Startup : FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{
			builder.Services.AddDbContext<BigBeerContext>(opt =>
									opt.UseSqlServer(
											builder.GetContext().Configuration["DBConnection"]
									));

			builder.Services.AddHttpClient("BeerBot", config =>
								{
									var productValue = new ProductInfoHeaderValue("BBD", "1.0");
									var commentValue = new ProductInfoHeaderValue("(Hi)");

									config.BaseAddress = new Uri("https://api.untappd.com/v4/");

									config.DefaultRequestHeaders.UserAgent.Add(productValue);
									config.DefaultRequestHeaders.UserAgent.Add(commentValue);
								});

		}
	}
}
