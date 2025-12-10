using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using WebApp.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();
builder.Services.AddScoped<BrowserService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:7071") });

await builder.Build().RunAsync();
