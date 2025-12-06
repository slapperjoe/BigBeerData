using System.Diagnostics;
using System.Net.Http.Headers;
using BigBeerData.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

IConfiguration config = new ConfigurationBuilder()
	.AddEnvironmentVariables()
	.Build();

var host = new HostBuilder()
	.ConfigureFunctionsWorkerDefaults()
	.ConfigureServices(services =>
	{
		services.AddDbContext<BigBeerContext>(opt => opt.UseSqlServer(config["DBConnection"]));

		services.AddHttpClient("BeerBot", clientConfig =>
		{
			var productValue = new ProductInfoHeaderValue("BBD", "1.0");
			var commentValue = new ProductInfoHeaderValue("(Hi)");

			var baseUrl = config["BeerBot:BaseUrl"] ?? config["UNTAPPD_BASE_URL"];
			if (string.IsNullOrWhiteSpace(baseUrl))
			{
				throw new InvalidOperationException("BeerBot:BaseUrl/UNTAPPD_BASE_URL is required");
			}

			clientConfig.BaseAddress = new Uri(baseUrl);
			clientConfig.DefaultRequestHeaders.UserAgent.Add(productValue);
			clientConfig.DefaultRequestHeaders.UserAgent.Add(commentValue);
		});

		services.AddOpenTelemetry()
			.ConfigureResource(res => res.AddService("BigBeerData.Api"))
			.WithTracing(tracing => tracing
				.AddSource("Api.Update")
				.AddHttpClientInstrumentation()
				.AddEntityFrameworkCoreInstrumentation(opt =>
				{
					opt.SetDbStatementForText = true;
					opt.SetDbStatementForStoredProcedure = true;
				})
				.AddOtlpExporter(exporter =>
				{
					var endpoint = config["OTLP_ENDPOINT"] ?? config["Telemetry:OtlpEndpoint"];
					if (!string.IsNullOrWhiteSpace(endpoint))
					{
						exporter.Endpoint = new Uri(endpoint);
					}
				}))
			.WithMetrics(metrics => metrics
				.AddRuntimeInstrumentation()
				.AddHttpClientInstrumentation()
				.AddOtlpExporter(exporter =>
				{
					var endpoint = config["OTLP_ENDPOINT"] ?? config["Telemetry:OtlpEndpoint"];
					if (!string.IsNullOrWhiteSpace(endpoint))
					{
						exporter.Endpoint = new Uri(endpoint);
					}
				}));
	})
	.Build();

await host.RunAsync();
