using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Sockets;

namespace MyApi.HealthChecks;

public class SmtpHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpHealthCheck> _logger;

    public SmtpHealthCheck(
        IConfiguration configuration,
        ILogger<SmtpHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPortString = _configuration["Email:SmtpPort"];

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpPortString))
            {
                return HealthCheckResult.Degraded(
                    "SMTP configuration not found. Email notifications will not work.");
            }

            if (!int.TryParse(smtpPortString, out var smtpPort))
            {
                return HealthCheckResult.Unhealthy("SMTP port configuration is invalid.");
            }

            // Test TCP connectivity to SMTP server
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(smtpHost, smtpPort);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                return HealthCheckResult.Degraded(
                    $"Connection to SMTP server {smtpHost}:{smtpPort} timed out.");
            }

            if (connectTask.IsFaulted)
            {
                return HealthCheckResult.Unhealthy(
                    $"Failed to connect to SMTP server {smtpHost}:{smtpPort}.",
                    exception: connectTask.Exception);
            }

            return HealthCheckResult.Healthy(
                $"SMTP server {smtpHost}:{smtpPort} is accessible.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking SMTP health");
            return HealthCheckResult.Unhealthy(
                "Error connecting to SMTP server.",
                exception: ex);
        }
    }
}
