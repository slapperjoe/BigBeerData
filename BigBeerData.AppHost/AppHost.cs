var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddConnectionString("bigbeerdb");
var storage = builder.AddConnectionString("AzureWebJobsStorage");

var api = builder.AddProject<Projects.Api>("api")
    .WithReference(db)
    .WithReference(storage);

builder.AddProject<Projects.WebApp>("webapp")
    .WithReference(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
