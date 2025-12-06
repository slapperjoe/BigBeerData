using BigBeerData.WebApp.Components;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddHttpClient("Api", client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.Configuration["API_BASE_URL"];
    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        throw new InvalidOperationException("ApiBaseUrl/API_BASE_URL is required");
    }

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(res => res.AddService("BigBeerData.WebApp"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            var endpoint = builder.Configuration["OTLP_ENDPOINT"] ?? builder.Configuration["Telemetry:OtlpEndpoint"];
            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                options.Endpoint = new Uri(endpoint);
            }
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter(options =>
        {
            var endpoint = builder.Configuration["OTLP_ENDPOINT"] ?? builder.Configuration["Telemetry:OtlpEndpoint"];
            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                options.Endpoint = new Uri(endpoint);
            }
        }));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();

app.Run();
