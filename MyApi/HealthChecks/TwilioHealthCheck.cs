using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MyApi.HealthChecks;

public class TwilioHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TwilioHealthCheck> _logger;

    public TwilioHealthCheck(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<TwilioHealthCheck> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];

            if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
            {
                return HealthCheckResult.Degraded(
                    "Twilio credentials not configured. SMS features will not work.");
            }

            // Test Twilio API connectivity
            var client = _httpClientFactory.CreateClient();
            var byteArray = System.Text.Encoding.ASCII.GetBytes($"{accountSid}:{authToken}");
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Basic", 
                    Convert.ToBase64String(byteArray));
            client.Timeout = TimeSpan.FromSeconds(5);

            var response = await client.GetAsync(
                $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}.json",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("Twilio API is accessible and credentials are valid.");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HealthCheckResult.Unhealthy("Twilio credentials are invalid.");
            }
            else
            {
                return HealthCheckResult.Degraded(
                    $"Twilio API returned status code: {response.StatusCode}");
            }
        }
        catch (TaskCanceledException)
        {
            return HealthCheckResult.Degraded("Twilio API health check timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Twilio health");
            return HealthCheckResult.Unhealthy(
                "Error connecting to Twilio API.",
                exception: ex);
        }
    }
}
