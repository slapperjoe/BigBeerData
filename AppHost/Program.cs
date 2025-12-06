using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Api>("api");
var web = builder.AddProject<Projects.WebApp>("webapp")
    .WithReference(api);

builder.Build().Run();
