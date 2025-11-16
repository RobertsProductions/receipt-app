using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyApi.DTOs;
using MyApi.Models;
using System.Security.Claims;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserProfileController> _logger;

    public UserProfileController(
        UserManager<ApplicationUser> userManager,
        ILogger<UserProfileController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        var profileDto = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            EmailConfirmed = user.EmailConfirmed,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            NotificationPreferences = new NotificationPreferencesDto
            {
                NotificationChannel = user.NotificationChannel.ToString(),
                NotificationThresholdDays = user.NotificationThresholdDays,
                OptOutOfNotifications = user.OptOutOfNotifications
            }
        };

        return Ok(profileDto);
    }

    /// <summary>
    /// Update user profile (FirstName, LastName)
    /// </summary>
    [HttpPut]
    public async Task<ActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {UserId} updated profile", userId);
            return Ok(new { message = "Profile updated successfully" });
        }

        return BadRequest(new { message = "Failed to update profile", errors = result.Errors });
    }

    /// <summary>
    /// Update user phone number
    /// </summary>
    [HttpPut("phone")]
    public async Task<ActionResult> UpdatePhoneNumber([FromBody] UpdatePhoneNumberDto dto)
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        // If phone number is being cleared
        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            user.PhoneNumber = null;
            user.PhoneNumberConfirmed = false;
            _logger.LogInformation("User {UserId} cleared phone number", userId);
        }
        else
        {
            // Validate phone number format (basic validation)
            var phoneNumber = dto.PhoneNumber.Trim();
            
            user.PhoneNumber = phoneNumber;
            user.PhoneNumberConfirmed = false; // Require re-confirmation when changed
            
            _logger.LogInformation("User {UserId} updated phone number to {Phone}", 
                userId, MaskPhoneNumber(phoneNumber));
        }

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            return Ok(new { 
                message = "Phone number updated successfully",
                phoneNumber = user.PhoneNumber,
                phoneNumberConfirmed = user.PhoneNumberConfirmed
            });
        }

        return BadRequest(new { message = "Failed to update phone number", errors = result.Errors });
    }

    /// <summary>
    /// Get notification preferences
    /// </summary>
    [HttpGet("preferences")]
    public async Task<ActionResult<NotificationPreferencesDto>> GetPreferences()
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        var preferences = new NotificationPreferencesDto
        {
            NotificationChannel = user.NotificationChannel.ToString(),
            NotificationThresholdDays = user.NotificationThresholdDays,
            OptOutOfNotifications = user.OptOutOfNotifications
        };

        return Ok(preferences);
    }

    /// <summary>
    /// Update notification preferences
    /// </summary>
    [HttpPut("preferences")]
    public async Task<ActionResult> UpdatePreferences([FromBody] UpdateNotificationPreferencesDto dto)
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        // Parse and validate notification channel
        if (!Enum.TryParse<NotificationChannel>(dto.NotificationChannel, true, out var channel))
        {
            return BadRequest(new { 
                message = "Invalid notification channel",
                validValues = Enum.GetNames<NotificationChannel>()
            });
        }

        // Validate that SMS channel is only selected if phone number is configured
        if ((channel == NotificationChannel.SmsOnly || channel == NotificationChannel.EmailAndSms) 
            && string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            return BadRequest(new { 
                message = "Cannot enable SMS notifications without a phone number configured",
                hint = "Please add your phone number first via PUT /api/userprofile/phone"
            });
        }

        user.NotificationChannel = channel;
        user.NotificationThresholdDays = dto.NotificationThresholdDays;
        user.OptOutOfNotifications = dto.OptOutOfNotifications;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {UserId} updated notification preferences: Channel={Channel}, Threshold={Days} days, OptOut={OptOut}",
                userId, channel, dto.NotificationThresholdDays, dto.OptOutOfNotifications);

            return Ok(new { 
                message = "Notification preferences updated successfully",
                preferences = new NotificationPreferencesDto
                {
                    NotificationChannel = user.NotificationChannel.ToString(),
                    NotificationThresholdDays = user.NotificationThresholdDays,
                    OptOutOfNotifications = user.OptOutOfNotifications
                }
            });
        }

        return BadRequest(new { message = "Failed to update preferences", errors = result.Errors });
    }

    /// <summary>
    /// Delete user account (soft delete - just opt out)
    /// </summary>
    [HttpDelete]
    public async Task<ActionResult> DeleteAccount()
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        // For now, just opt out of notifications
        // In production, you might want to:
        // 1. Delete user data (GDPR compliance)
        // 2. Anonymize user receipts
        // 3. Actually delete the account
        user.OptOutOfNotifications = true;
        
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            _logger.LogWarning("User {UserId} requested account deletion (opted out of notifications)", userId);
            return Ok(new { 
                message = "Account deletion requested. You have been opted out of notifications.",
                note = "To fully delete your account, please contact support."
            });
        }

        return BadRequest(new { message = "Failed to process deletion request", errors = result.Errors });
    }

    private string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 4)
            return "****";
        
        return $"****{phoneNumber.Substring(phoneNumber.Length - 4)}";
    }
}
