var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server resource with persistent lifetime
// The container will wait for SQL Server to be ready before starting dependent services
var sqlServer = builder.AddSqlServer("sqlserver")
    .WithLifetime(ContainerLifetime.Persistent);
    
var receiptdb = sqlServer.AddDatabase("receiptdb");

// Add SQLite database reference from configuration/user secrets (file-based, no container needed)
// Falls back to local file if not configured
var sqliteConnectionString = builder.Configuration["ConnectionStrings:sqlitedb"] 
    ?? "Data Source=receipts.db";
var sqlitedb = builder.AddConnectionString("sqlitedb", sqliteConnectionString);

// Add OpenAI API key as a parameter that can be configured in Aspire Dashboard
// Falls back to user secrets or configuration
var openAiApiKey = builder.AddParameter("openai-apikey", secret: true);

// Add the API with database connections
// WaitFor ensures SQL Server container is ready before starting the API
var myApi = builder.AddProject<Projects.MyApi>("myapi")
    .WithReference(receiptdb)
    .WithReference(sqlitedb)
    .WithEnvironment("OpenAI__ApiKey", openAiApiKey)
    .WithExternalHttpEndpoints()
    .WaitFor(receiptdb);

// Add Angular frontend as NPM app
// Runs 'npm start' in the WarrantyApp.Web directory
// Aspire will allocate a port and pass it via PORT environment variable
// Angular will use that port (no --port 0, just let Aspire control it)
var frontend = builder.AddNpmApp("frontend", "../WarrantyApp.Web", "start")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WithReference(myApi)
    .WithEnvironment("API_URL", myApi.GetEndpoint("http"))
    .WaitFor(myApi);

builder.Build().Run();
