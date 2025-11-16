using Microsoft.AspNetCore.Identity;
using MyApi.Models;

namespace MyApi.Services;

/// <summary>
/// Composite notification service that sends both email and SMS notifications.
/// Fetches user's email and phone number from Identity and sends appropriate notifications.
/// </summary>
public class CompositeNotificationService : INotificationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EmailNotificationService _emailService;
    private readonly SmsNotificationService _smsService;
    private readonly ILogger<CompositeNotificationService> _logger;

    public CompositeNotificationService(
        UserManager<ApplicationUser> userManager,
        EmailNotificationService emailService,
        SmsNotificationService smsService,
        ILogger<CompositeNotificationService> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }

    public async Task SendWarrantyExpirationNotificationAsync(
        string userId,
        string userEmail,
        string productName,
        DateTime expirationDate,
        Guid receiptId)
    {
        var daysUntilExpiration = (expirationDate.Date - DateTime.UtcNow.Date).Days;
        
        _logger.LogInformation("Sending composite notification for user {UserId} - {Product} expiring in {Days} days",
            userId, productName, daysUntilExpiration);

        // Fetch full user details to get phone number and preferences
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found, cannot send notifications", userId);
            return;
        }

        // Check if user opted out
        if (user.OptOutOfNotifications)
        {
            _logger.LogInformation("User {UserId} has opted out of notifications, skipping", userId);
            return;
        }

        // Check notification channel preference
        var channel = user.NotificationChannel;
        
        if (channel == NotificationChannel.None)
        {
            _logger.LogInformation("User {UserId} has notifications disabled, skipping", userId);
            return;
        }

        var tasks = new List<Task>();

        // Send email notification if enabled
        if ((channel == NotificationChannel.EmailOnly || channel == NotificationChannel.EmailAndSms) 
            && !string.IsNullOrWhiteSpace(user.Email))
        {
            tasks.Add(SendEmailNotificationAsync(user.Email, productName, expirationDate, receiptId));
        }
        else if (channel == NotificationChannel.EmailOnly || channel == NotificationChannel.EmailAndSms)
        {
            _logger.LogWarning("User {UserId} wants email notifications but has no email address", userId);
        }

        // Send SMS notification if enabled and phone number is configured
        if ((channel == NotificationChannel.SmsOnly || channel == NotificationChannel.EmailAndSms)
            && !string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            tasks.Add(SendSmsNotificationAsync(user.PhoneNumber, productName, expirationDate, daysUntilExpiration));
        }
        else if (channel == NotificationChannel.SmsOnly || channel == NotificationChannel.EmailAndSms)
        {
            _logger.LogDebug("User {UserId} wants SMS notifications but has no phone number configured", userId);
        }

        if (!tasks.Any())
        {
            _logger.LogWarning("No notification channels available for user {UserId}", userId);
            return;
        }

        // Send all notifications in parallel
        await Task.WhenAll(tasks);

        _logger.LogInformation("Composite notification completed for user {UserId}", userId);
    }

    private async Task SendEmailNotificationAsync(string email, string productName, DateTime expirationDate, Guid receiptId)
    {
        try
        {
            await _emailService.SendWarrantyExpirationNotificationAsync(
                string.Empty, // userId not needed by email service
                email,
                productName,
                expirationDate,
                receiptId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification to {Email}", email);
            // Don't throw - we want to try SMS even if email fails
        }
    }

    private async Task SendSmsNotificationAsync(string phoneNumber, string productName, DateTime expirationDate, int daysUntilExpiration)
    {
        try
        {
            await _smsService.SendWarrantyExpirationSmsAsync(
                phoneNumber,
                productName,
                expirationDate,
                daysUntilExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS notification to {Phone}", MaskPhoneNumber(phoneNumber));
            // Don't throw - failures shouldn't break the notification flow
        }
    }

    private string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 4)
            return "****";
        
        return $"****{phoneNumber.Substring(phoneNumber.Length - 4)}";
    }
}
