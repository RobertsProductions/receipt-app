using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyApi.DTOs;
using MyApi.Models;
using MyApi.Services;
using System.Security.Claims;

namespace MyApi.Controllers;

/// <summary>
/// User profile management endpoints for updating personal information, phone verification, and notification preferences.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserProfileController> _logger;
    private readonly IPhoneVerificationService _phoneVerificationService;

    public UserProfileController(
        UserManager<ApplicationUser> userManager,
        ILogger<UserProfileController> logger,
        IPhoneVerificationService phoneVerificationService)
    {
        _userManager = userManager;
        _logger = logger;
        _phoneVerificationService = phoneVerificationService;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

    /// <summary>
    /// Retrieves the current authenticated user's complete profile.
    /// </summary>
    /// <returns>User profile including personal info, phone status, and notification preferences</returns>
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
    /// Updates user profile information (first name and last name).
    /// </summary>
    /// <param name="dto">Updated first name and last name</param>
    /// <returns>Success message on successful update</returns>
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
    /// Updates or removes the user's phone number (requires re-verification after change).
    /// </summary>
    /// <param name="dto">New phone number (or empty to remove)</param>
    /// <returns>Updated phone number and confirmation status</returns>
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
    /// Sends a 6-digit verification code via SMS to the specified phone number.
    /// </summary>
    /// <param name="dto">Phone number to verify</param>
    /// <returns>Success message with masked phone number and expiration time (5 minutes)</returns>
    [HttpPost("phone/verify/send")]
    public async Task<ActionResult> SendPhoneVerification([FromBody] SendPhoneVerificationDto dto)
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        // Validate phone number format
        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            return BadRequest(new { message = "Phone number is required" });
        }

        var phoneNumber = dto.PhoneNumber.Trim();

        // Send verification code
        var (success, message) = await _phoneVerificationService.SendVerificationCodeAsync(userId, phoneNumber);

        if (success)
        {
            _logger.LogInformation("Verification code sent to user {UserId}", userId);
            return Ok(new { 
                message = message,
                phoneNumber = MaskPhoneNumber(phoneNumber),
                expiresIn = "5 minutes"
            });
        }

        return BadRequest(new { message = message });
    }

    /// <summary>
    /// Verifies phone number using the SMS code sent earlier.
    /// </summary>
    /// <param name="dto">6-digit verification code received via SMS</param>
    /// <returns>Success message on successful verification (3 attempts max, 5 minute expiration)</returns>
    [HttpPost("phone/verify/confirm")]
    public async Task<ActionResult> VerifyPhoneNumber([FromBody] VerifyPhoneDto dto)
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        // Verify the code
        var isValid = await _phoneVerificationService.VerifyCodeAsync(userId, dto.VerificationCode);

        if (isValid)
        {
            // Mark phone number as confirmed
            user.PhoneNumberConfirmed = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("Phone number verified for user {UserId}", userId);
                return Ok(new { 
                    message = "Phone number verified successfully",
                    phoneNumber = MaskPhoneNumber(user.PhoneNumber ?? ""),
                    phoneNumberConfirmed = true
                });
            }

            return BadRequest(new { message = "Failed to update verification status", errors = result.Errors });
        }

        return BadRequest(new { 
            message = "Invalid or expired verification code",
            hint = "Please request a new code if yours has expired or you've exceeded the maximum attempts (3)"
        });
    }

    /// <summary>
    /// Retrieves the user's notification preferences.
    /// </summary>
    /// <returns>Notification channel, threshold days, and opt-out status</returns>
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
    /// Updates notification preferences for warranty expiration alerts.
    /// </summary>
    /// <param name="dto">Notification channel (EmailOnly, SmsOnly, EmailAndSms), threshold days, and opt-out flag</param>
    /// <returns>Updated notification preferences</returns>
    /// <remarks>
    /// SMS channels require a verified phone number. Threshold determines how many days before expiration to send notifications.
    /// </remarks>
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
    /// Requests account deletion by opting out of notifications (soft delete).
    /// </summary>
    /// <returns>Success message with instructions for full account deletion</returns>
    /// <remarks>
    /// Currently opts user out of notifications. For full GDPR-compliant deletion, contact support.
    /// </remarks>
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
