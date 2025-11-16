using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Http;

namespace MyApi.HealthChecks;

public class OpenAiHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OpenAiHealthCheck> _logger;

    public OpenAiHealthCheck(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<OpenAiHealthCheck> logger)
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
            var apiKey = _configuration["OpenAI:ApiKey"];
            
            if (string.IsNullOrEmpty(apiKey))
            {
                return HealthCheckResult.Degraded(
                    "OpenAI API key not configured. OCR features will not work.");
            }

            // Quick validation of API key format
            if (!apiKey.StartsWith("sk-"))
            {
                return HealthCheckResult.Unhealthy(
                    "OpenAI API key format appears invalid.");
            }

            // Test connectivity to OpenAI API
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            client.Timeout = TimeSpan.FromSeconds(5);

            var response = await client.GetAsync(
                "https://api.openai.com/v1/models",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("OpenAI API is accessible and API key is valid.");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HealthCheckResult.Unhealthy("OpenAI API key is invalid or expired.");
            }
            else
            {
                return HealthCheckResult.Degraded(
                    $"OpenAI API returned status code: {response.StatusCode}");
            }
        }
        catch (TaskCanceledException)
        {
            return HealthCheckResult.Degraded("OpenAI API health check timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking OpenAI health");
            return HealthCheckResult.Unhealthy(
                "Error connecting to OpenAI API.",
                exception: ex);
        }
    }
}
