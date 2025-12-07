using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

// Minimal stubs to mirror template AddServiceDefaults/MapDefaultEndpoints when a ServiceDefaults project isn't included.
internal static class ServiceDefaultsExtensions
{
    public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder)
    {
        // In the template, this wires health checks, logging, tracing defaults. No-op here.
        return builder;
    }

    public static void MapDefaultEndpoints(this WebApplication app)
    {
        // In the template, this maps health/metrics endpoints. No-op here.
    }
}
