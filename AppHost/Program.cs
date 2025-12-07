using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject("api", "../Api/Api.csproj");
var web = builder.AddProject("webapp", "../WebApp/WebApp.csproj")
    .WithReference(api);

builder.Build().Run();
