var builder = DistributedApplication.CreateBuilder(args);

var myApi = builder.AddProject<Projects.MyApi>("myapi");

builder.Build().Run();
