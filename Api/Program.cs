using Microsoft.Extensions.Hosting;


using BigBeerData.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using System.Net.Http.Headers;

IConfiguration config = new ConfigurationBuilder()
	 .AddEnvironmentVariables()
	 .Build();

var host = new HostBuilder()
		.ConfigureFunctionsWorkerDefaults()
		  .ConfigureServices(builder =>
		  {
			  builder.AddDbContext<BigBeerContext>(opt =>
											  opt.UseSqlServer(
													config["DBConnection"]
											  ));
			  builder.AddHttpClient("BeerBot", config =>
				{
					var productValue = new ProductInfoHeaderValue("BBD", "1.0");
					var commentValue = new ProductInfoHeaderValue("(Hi)");

					config.BaseAddress = new Uri("https://api.untappd.com/v4/");

					config.DefaultRequestHeaders.UserAgent.Add(productValue);
					config.DefaultRequestHeaders.UserAgent.Add(commentValue);
				});
		  })
		.Build();

await host.RunAsync();
