using System.ComponentModel.DataAnnotations;

namespace MyApi.DTOs;

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public NotificationPreferencesDto NotificationPreferences { get; set; } = new();
}

public class UpdateProfileDto
{
    [StringLength(50)]
    public string? FirstName { get; set; }
    
    [StringLength(50)]
    public string? LastName { get; set; }
}

public class UpdatePhoneNumberDto
{
    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
}

public class NotificationPreferencesDto
{
    public string NotificationChannel { get; set; } = "EmailAndSms";
    public int NotificationThresholdDays { get; set; } = 7;
    public bool OptOutOfNotifications { get; set; } = false;
}

public class UpdateNotificationPreferencesDto
{
    [Required]
    public string NotificationChannel { get; set; } = "EmailAndSms";
    
    [Range(1, 90)]
    public int NotificationThresholdDays { get; set; } = 7;
    
    public bool OptOutOfNotifications { get; set; } = false;
}
