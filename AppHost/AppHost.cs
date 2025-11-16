var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server resource
var sqlServer = builder.AddSqlServer("sqlserver")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("receiptdb");

// Add SQLite database (file-based, no container needed)
var sqlite = builder.AddConnectionString("sqlite", "Data Source=receipts.db");

// Add the API with database connections
var myApi = builder.AddProject<Projects.MyApi>("myapi")
    .WithReference(sqlServer)
    .WithReference(sqlite);

builder.Build().Run();
