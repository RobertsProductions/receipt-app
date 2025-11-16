namespace MyApi.Services;

/// <summary>
/// SMS notification service using Twilio.
/// Only sends SMS if user has a phone number configured in their profile.
/// </summary>
public class SmsNotificationService : INotificationService
{
    private readonly ILogger<SmsNotificationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string? _accountSid;
    private readonly string? _authToken;
    private readonly string? _fromPhoneNumber;
    private readonly bool _isConfigured;

    public SmsNotificationService(ILogger<SmsNotificationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        // Load Twilio configuration (optional)
        var twilioConfig = configuration.GetSection("Twilio");
        _accountSid = twilioConfig["AccountSid"];
        _authToken = twilioConfig["AuthToken"];
        _fromPhoneNumber = twilioConfig["FromPhoneNumber"];

        _isConfigured = !string.IsNullOrWhiteSpace(_accountSid) &&
                       !string.IsNullOrWhiteSpace(_authToken) &&
                       !string.IsNullOrWhiteSpace(_fromPhoneNumber);

        if (_isConfigured)
        {
            _logger.LogInformation("SMS notification service initialized with Twilio (from: {Phone})", _fromPhoneNumber);
        }
        else
        {
            _logger.LogWarning("SMS notification service not configured - Twilio credentials missing");
        }
    }

    public async Task SendWarrantyExpirationNotificationAsync(
        string userId,
        string userEmail,
        string productName,
        DateTime expirationDate,
        Guid receiptId)
    {
        if (!_isConfigured)
        {
            _logger.LogDebug("SMS notifications disabled - Twilio not configured");
            return;
        }

        // Note: This method receives userEmail but we need phoneNumber
        // The CompositeNotificationService or caller should pass the phone number
        // For now, we'll log that we can't send without phone number
        // In a real implementation, you'd need to fetch the user's phone number from the database
        
        _logger.LogWarning("SMS notification skipped: Phone number not provided in notification parameters");
        _logger.LogInformation("To enable SMS notifications, extend INotificationService to include phoneNumber parameter");
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Send SMS to a specific phone number with custom message.
    /// Used for verification codes and custom notifications.
    /// </summary>
    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        if (!_isConfigured)
        {
            _logger.LogDebug("SMS not sent - Twilio not configured");
            return await Task.FromResult(false);
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            _logger.LogDebug("SMS not sent - no phone number provided");
            return await Task.FromResult(false);
        }

        try
        {
            // In a real implementation with Twilio SDK:
            // var twilio = new TwilioRestClient(_accountSid, _authToken);
            // var result = await twilio.MessageResource.CreateAsync(
            //     to: new PhoneNumber(phoneNumber),
            //     from: new PhoneNumber(_fromPhoneNumber),
            //     body: message
            // );

            // For now, we'll just log it (simulated SMS)
            _logger.LogInformation("SMS (simulated) sent to {Phone}: {Message}", 
                MaskPhoneNumber(phoneNumber), message);

            // To enable actual SMS sending:
            // 1. Add Twilio NuGet package: Twilio
            // 2. Uncomment the code above
            // 3. Configure Twilio credentials in appsettings or user secrets
            // 4. Return result.Status == "sent" or similar

            return await Task.FromResult(true); // Simulated success
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {Phone}", MaskPhoneNumber(phoneNumber));
            return await Task.FromResult(false);
        }
    }

    // Alternative method that accepts phone number explicitly
    public async Task SendWarrantyExpirationSmsAsync(
        string phoneNumber,
        string productName,
        DateTime expirationDate,
        int daysUntilExpiration)
    {
        if (!_isConfigured)
        {
            _logger.LogDebug("SMS notifications disabled - Twilio not configured");
            return;
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            _logger.LogDebug("SMS notification skipped - no phone number provided");
            return;
        }

        try
        {
            var message = GenerateSmsMessage(productName, expirationDate, daysUntilExpiration);

            // In a real implementation with Twilio SDK:
            // var twilio = new TwilioRestClient(_accountSid, _authToken);
            // var result = await twilio.MessageResource.CreateAsync(
            //     to: new PhoneNumber(phoneNumber),
            //     from: new PhoneNumber(_fromPhoneNumber),
            //     body: message
            // );

            // For now, we'll just log it
            _logger.LogInformation("SMS notification (simulated) sent to {Phone}: {Message}", 
                MaskPhoneNumber(phoneNumber), message);

            // To enable actual SMS sending:
            // 1. Add Twilio NuGet package: Twilio
            // 2. Uncomment the code above
            // 3. Configure Twilio credentials in appsettings or user secrets
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS notification to {Phone}", MaskPhoneNumber(phoneNumber));
        }

        await Task.CompletedTask;
    }

    public async Task SendReceiptSharedNotificationAsync(
        string recipientUserId,
        string recipientEmail,
        string ownerName,
        string receiptFileName,
        Guid receiptId,
        string? shareNote)
    {
        if (!_isConfigured)
        {
            _logger.LogDebug("SMS notifications disabled - Twilio not configured");
            return;
        }

        _logger.LogInformation("SMS notification for receipt sharing not implemented - phone number needs to be passed");
        _logger.LogDebug("Receipt {ReceiptId} shared with user {UserId} by {Owner}", receiptId, recipientUserId, ownerName);
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Send SMS notification for receipt sharing to a specific phone number
    /// </summary>
    public async Task SendReceiptSharedSmsAsync(
        string phoneNumber,
        string ownerName,
        string receiptFileName)
    {
        if (!_isConfigured)
        {
            _logger.LogDebug("SMS notifications disabled - Twilio not configured");
            return;
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            _logger.LogDebug("SMS notification skipped - no phone number provided");
            return;
        }

        try
        {
            var message = $"📄 {ownerName} shared a receipt with you: '{receiptFileName}'. Check your Warranty App to view it.";

            _logger.LogInformation("Receipt shared SMS (simulated) sent to {Phone}: {Message}", 
                MaskPhoneNumber(phoneNumber), message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send receipt shared SMS to {Phone}", MaskPhoneNumber(phoneNumber));
        }

        await Task.CompletedTask;
    }

    private string GenerateSmsMessage(string productName, DateTime expirationDate, int daysUntilExpiration)
    {
        var urgency = daysUntilExpiration <= 3 ? "URGENT: " : "";
        return $"{urgency}Warranty Alert: Your warranty for '{productName}' expires in {daysUntilExpiration} day(s) on {expirationDate:MM/dd/yyyy}. Review your coverage options soon.";
    }

    private string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 4)
            return "****";
        
        return $"****{phoneNumber.Substring(phoneNumber.Length - 4)}";
    }
}
