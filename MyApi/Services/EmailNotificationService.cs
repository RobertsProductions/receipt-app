namespace MyApi.Services;

/// <summary>
/// Email notification service using SMTP.
/// Supports multiple SMTP providers (Gmail, SendGrid, AWS SES, etc.)
/// </summary>
public class EmailNotificationService : INotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly bool _useSsl;

    public EmailNotificationService(ILogger<EmailNotificationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        // Load SMTP configuration
        var smtpConfig = configuration.GetSection("Smtp");
        _smtpHost = smtpConfig["Host"] ?? throw new InvalidOperationException("SMTP Host not configured");
        _smtpPort = smtpConfig.GetValue<int>("Port", 587);
        _smtpUsername = smtpConfig["Username"] ?? throw new InvalidOperationException("SMTP Username not configured");
        _smtpPassword = smtpConfig["Password"] ?? throw new InvalidOperationException("SMTP Password not configured");
        _fromEmail = smtpConfig["FromEmail"] ?? _smtpUsername;
        _fromName = smtpConfig["FromName"] ?? "Warranty App";
        _useSsl = smtpConfig.GetValue<bool>("UseSsl", true);

        _logger.LogInformation("Email notification service initialized with SMTP host: {Host}:{Port}", _smtpHost, _smtpPort);
    }

    public async Task SendWarrantyExpirationNotificationAsync(
        string userId,
        string userEmail,
        string productName,
        DateTime expirationDate,
        Guid receiptId)
    {
        if (string.IsNullOrWhiteSpace(userEmail))
        {
            _logger.LogWarning("Cannot send email notification: User {UserId} has no email address", userId);
            return;
        }

        var daysUntilExpiration = (expirationDate.Date - DateTime.UtcNow.Date).Days;

        try
        {
            using var smtpClient = new System.Net.Mail.SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = _useSsl,
                Credentials = new System.Net.NetworkCredential(_smtpUsername, _smtpPassword),
                Timeout = 30000 // 30 seconds
            };

            var subject = $"⚠️ Warranty Expiring Soon: {productName}";
            var body = GenerateEmailBody(productName, expirationDate, daysUntilExpiration, receiptId);

            var mailMessage = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(userEmail);

            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation("Email notification sent to {Email} for receipt {ReceiptId} - {Product} expiring in {Days} days",
                userEmail, receiptId, productName, daysUntilExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification to {Email} for receipt {ReceiptId}", userEmail, receiptId);
            throw;
        }
    }

    private string GenerateEmailBody(string productName, DateTime expirationDate, int daysUntilExpiration, Guid receiptId)
    {
        var urgencyLevel = daysUntilExpiration <= 3 ? "URGENT" : daysUntilExpiration <= 7 ? "Important" : "Notice";
        var urgencyColor = daysUntilExpiration <= 3 ? "#dc3545" : daysUntilExpiration <= 7 ? "#ffc107" : "#17a2b8";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: {urgencyColor}; color: white; padding: 15px; border-radius: 5px 5px 0 0;'>
        <h2 style='margin: 0;'>⚠️ {urgencyLevel}: Warranty Expiring Soon</h2>
    </div>
    
    <div style='background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; border-top: none; border-radius: 0 0 5px 5px;'>
        <p style='font-size: 16px; margin-top: 0;'>
            Your warranty for <strong>{productName}</strong> will expire in:
        </p>
        
        <div style='background-color: white; padding: 15px; margin: 15px 0; border-left: 4px solid {urgencyColor}; border-radius: 4px;'>
            <h1 style='margin: 0; color: {urgencyColor}; font-size: 36px;'>{daysUntilExpiration} Day{(daysUntilExpiration != 1 ? "s" : "")}</h1>
            <p style='margin: 5px 0 0 0; color: #6c757d;'>Expiration Date: {expirationDate:MMMM dd, yyyy}</p>
        </div>
        
        <h3>What should you do?</h3>
        <ul style='padding-left: 20px;'>
            <li>Review your receipt and warranty terms</li>
            <li>Contact the manufacturer or retailer about warranty renewal options</li>
            <li>Consider extended warranty coverage if available</li>
            <li>File any pending warranty claims before expiration</li>
        </ul>
        
        <div style='margin-top: 20px; padding-top: 20px; border-top: 1px solid #dee2e6; font-size: 14px; color: #6c757d;'>
            <p style='margin: 5px 0;'><strong>Receipt ID:</strong> {receiptId}</p>
            <p style='margin: 5px 0;'>This is an automated notification from your Warranty Management System.</p>
        </div>
    </div>
    
    <div style='margin-top: 20px; text-align: center; font-size: 12px; color: #6c757d;'>
        <p>© {DateTime.UtcNow.Year} Warranty App. All rights reserved.</p>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Send a generic email with custom subject and body
    /// </summary>
    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("Cannot send email: No recipient email address provided");
            return;
        }

        try
        {
            using var smtpClient = new System.Net.Mail.SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = _useSsl,
                Credentials = new System.Net.NetworkCredential(_smtpUsername, _smtpPassword),
                Timeout = 30000 // 30 seconds
            };

            var mailMessage = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email} with subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }
}
