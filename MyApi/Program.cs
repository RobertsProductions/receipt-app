using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyApi.Data;
using MyApi.Models;
using MyApi.Services;
using MyApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add Response Caching
builder.Services.AddResponseCaching();

// Add Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = new[]
    {
        "application/json",
        "application/xml",
        "text/plain",
        "text/html",
        "text/css",
        "text/javascript",
        "application/javascript",
        "image/svg+xml"
    };
});

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Receipt App API",
        Version = "v1",
        Description = "API for warranty and receipt management with authentication"
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Database
var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";
var connectionString = databaseProvider switch
{
    "Sqlite" => builder.Configuration.GetConnectionString("SqliteConnection") 
                ?? builder.Configuration.GetConnectionString("sqlitedb") 
                ?? "Data Source=receipts.db",
    "SqlServer" => builder.Configuration.GetConnectionString("SqlServerConnection") 
                   ?? builder.Configuration.GetConnectionString("receiptdb") 
                   ?? builder.Configuration.GetConnectionString("DefaultConnection"),
    _ => builder.Configuration.GetConnectionString("DefaultConnection")
} ?? throw new InvalidOperationException($"Connection string for {databaseProvider} not found");

// Log which database provider and connection string is being used
builder.Services.AddSingleton<ILogger>(sp => sp.GetRequiredService<ILogger<Program>>());
var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation("Using database provider: {Provider}", databaseProvider);
logger.LogInformation("Connection string: {ConnectionString}", connectionString.Replace("Password=", "Password=***"));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (databaseProvider == "Sqlite")
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        // Add connection resiliency for SQL Server to handle transient failures
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(60); // Increase command timeout to 60 seconds
        });
    }
});

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

var secret = jwtSettings.Get<JwtSettings>()?.Secret ?? throw new InvalidOperationException("JWT Secret not configured");
var key = Encoding.UTF8.GetBytes(secret);

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Get<JwtSettings>()?.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Get<JwtSettings>()?.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Register notification services
// Choose ONE of the following notification service registrations:
// 1. Log only (development/testing)
// builder.Services.AddSingleton<INotificationService, LogNotificationService>();

// 2. Email only (production without SMS)
// builder.Services.AddSingleton<EmailNotificationService>();
// builder.Services.AddSingleton<INotificationService>(sp => sp.GetRequiredService<EmailNotificationService>());

// 3. Composite (Email + SMS) - recommended for production
builder.Services.AddSingleton<EmailNotificationService>();
builder.Services.AddSingleton<SmsNotificationService>();
builder.Services.AddScoped<INotificationService, CompositeNotificationService>();

// Register other services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IOcrService, OpenAiOcrService>();
builder.Services.AddScoped<IPhoneVerificationService, PhoneVerificationService>();
builder.Services.AddScoped<IChatbotService, ChatbotService>();

// Register background services
builder.Services.AddHostedService<WarrantyExpirationService>();

// Add memory cache for warranty notifications
builder.Services.AddMemoryCache();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add HttpClient for health checks
builder.Services.AddHttpClient();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(
        name: "database",
        tags: new[] { "db", "sql" })
    .AddCheck<MyApi.HealthChecks.OpenAiHealthCheck>(
        name: "openai",
        tags: new[] { "external", "ocr" })
    .AddCheck<MyApi.HealthChecks.SmtpHealthCheck>(
        name: "smtp",
        tags: new[] { "external", "email" })
    .AddCheck<MyApi.HealthChecks.TwilioHealthCheck>(
        name: "twilio",
        tags: new[] { "external", "sms" })
    .AddCheck<MyApi.HealthChecks.FileStorageHealthCheck>(
        name: "filestorage",
        tags: new[] { "storage" });

var app = builder.Build();

// Apply migrations automatically in development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var migrationLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            migrationLogger.LogInformation("Applying database migrations...");
            dbContext.Database.Migrate();
            migrationLogger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            migrationLogger.LogError(ex, "An error occurred while applying migrations.");
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add Response Compression (before static files and routing)
app.UseResponseCompression();

// Add Response Caching
app.UseResponseCaching();

// Add Rate Limiting
app.UseMiddleware<RateLimitingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map Health Check Endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.TotalMilliseconds,
                data = entry.Value.Data,
                tags = entry.Value.Tags
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

// Detailed health check endpoint
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db") || check.Tags.Contains("storage"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.TotalMilliseconds
            })
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

// Liveness check (basic ping)
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // No checks, just returns 200 OK if app is running
});

// Keep the weather forecast endpoint for testing
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Make Program class accessible for integration tests
public partial class Program { }

