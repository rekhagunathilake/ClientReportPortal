var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.ClientReportPortal_Api>("api");

builder.AddProject<Projects.ClientReportPortal_Web>("web")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
