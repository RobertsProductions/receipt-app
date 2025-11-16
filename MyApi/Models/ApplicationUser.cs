using Microsoft.AspNetCore.Identity;

namespace MyApi.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    
    // Notification preferences
    public NotificationChannel NotificationChannel { get; set; } = NotificationChannel.EmailAndSms;
    public int NotificationThresholdDays { get; set; } = 7;
    public bool OptOutOfNotifications { get; set; } = false;
}

public enum NotificationChannel
{
    None = 0,
    EmailOnly = 1,
    SmsOnly = 2,
    EmailAndSms = 3
}
