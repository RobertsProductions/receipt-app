namespace MyApi.Services;

public interface IPhoneVerificationService
{
    Task<(bool Success, string Message)> SendVerificationCodeAsync(string userId, string phoneNumber);
    Task<bool> VerifyCodeAsync(string userId, string code);
}
