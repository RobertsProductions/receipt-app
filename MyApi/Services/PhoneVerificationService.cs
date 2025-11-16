using System.Collections.Concurrent;
using MyApi.Models;

namespace MyApi.Services;

public class PhoneVerificationService : IPhoneVerificationService
{
    private readonly SmsNotificationService _smsService;
    private readonly ILogger<PhoneVerificationService> _logger;
    
    // In-memory storage for verification codes (use Redis/database in production)
    private static readonly ConcurrentDictionary<string, VerificationCodeEntry> _verificationCodes = new();
    
    public PhoneVerificationService(
        SmsNotificationService smsService,
        ILogger<PhoneVerificationService> logger)
    {
        _smsService = smsService;
        _logger = logger;
    }

    public async Task<(bool Success, string Message)> SendVerificationCodeAsync(string userId, string phoneNumber)
    {
        try
        {
            // Generate 6-digit verification code
            var code = GenerateVerificationCode();
            
            // Store code with expiration (5 minutes)
            var entry = new VerificationCodeEntry
            {
                Code = code,
                PhoneNumber = phoneNumber,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                AttemptCount = 0
            };
            
            _verificationCodes[userId] = entry;
            
            // Send SMS with verification code
            var message = $"Your warranty app verification code is: {code}. Valid for 5 minutes.";
            var smsResult = await _smsService.SendSmsAsync(phoneNumber, message);
            
            if (smsResult)
            {
                _logger.LogInformation("Verification code sent to user {UserId} at {Phone}", 
                    userId, MaskPhoneNumber(phoneNumber));
                return (true, "Verification code sent successfully");
            }
            else
            {
                _verificationCodes.TryRemove(userId, out _);
                _logger.LogWarning("Failed to send verification code to user {UserId}", userId);
                return (false, "Failed to send verification code. Please ensure SMS notifications are configured.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification code for user {UserId}", userId);
            return (false, "An error occurred while sending verification code");
        }
    }

    public Task<bool> VerifyCodeAsync(string userId, string code)
    {
        try
        {
            if (!_verificationCodes.TryGetValue(userId, out var entry))
            {
                _logger.LogWarning("No verification code found for user {UserId}", userId);
                return Task.FromResult(false);
            }

            // Check if code has expired
            if (DateTime.UtcNow > entry.ExpiresAt)
            {
                _verificationCodes.TryRemove(userId, out _);
                _logger.LogWarning("Verification code expired for user {UserId}", userId);
                return Task.FromResult(false);
            }

            // Check attempt count (max 3 attempts)
            if (entry.AttemptCount >= 3)
            {
                _verificationCodes.TryRemove(userId, out _);
                _logger.LogWarning("Max verification attempts exceeded for user {UserId}", userId);
                return Task.FromResult(false);
            }

            entry.AttemptCount++;

            // Verify code
            if (entry.Code == code)
            {
                _verificationCodes.TryRemove(userId, out _);
                _logger.LogInformation("Phone number verified successfully for user {UserId}", userId);
                return Task.FromResult(true);
            }

            _logger.LogWarning("Invalid verification code for user {UserId}. Attempt {Attempt}/3", 
                userId, entry.AttemptCount);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying code for user {UserId}", userId);
            return Task.FromResult(false);
        }
    }

    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 4)
            return "****";
        
        return $"****{phoneNumber.Substring(phoneNumber.Length - 4)}";
    }

    private class VerificationCodeEntry
    {
        public string Code { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public int AttemptCount { get; set; }
    }
}
