using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MyApi.DTOs;
using MyApi.Models;
using MyApi.Services;
using System.Text;
using System.Text.Encodings.Web;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly EmailNotificationService _emailService;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        EmailNotificationService emailService,
        ILogger<AuthController> logger,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new ApplicationUser
        {
            UserName = model.Username,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return BadRequest(ModelState);
        }

        // Generate email confirmation token
        var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmationToken));
        
        // Build confirmation URL
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
        var confirmationLink = $"{baseUrl}/api/Auth/confirm-email?userId={user.Id}&token={encodedToken}";
        
        // Send confirmation email
        try
        {
            var emailBody = GenerateConfirmationEmailBody(user.Email!, confirmationLink);
            await _emailService.SendEmailAsync(
                user.Email!,
                "Confirm Your Email - Warranty App",
                emailBody
            );
            _logger.LogInformation("Confirmation email sent to {Email}", model.Email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send confirmation email to {Email}", model.Email);
            // Don't fail registration if email fails
        }

        _logger.LogInformation("User {Email} registered successfully", model.Email);

        var token = _tokenService.GenerateToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(60);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = refreshTokenExpiresAt;
        await _userManager.UpdateAsync(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            Email = user.Email!,
            Username = user.UserName!,
            ExpiresAt = expiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        });
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // Check if 2FA is enabled
        if (user.TwoFactorEnabled)
        {
            // Verify password is correct
            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordValid)
            {
                _logger.LogWarning("Failed login attempt for {Email}", model.Email);
                return Unauthorized(new { message = "Invalid email or password" });
            }

            return BadRequest(new 
            { 
                message = "2FA is enabled for this account. Please use the /api/Auth/login/2fa endpoint with your 2FA code",
                requires2FA = true 
            });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed login attempt for {Email}", model.Email);
            return Unauthorized(new { message = "Invalid email or password" });
        }

        user.LastLoginAt = DateTime.UtcNow;
        
        var token = _tokenService.GenerateToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(60);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = refreshTokenExpiresAt;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {Email} logged in successfully", model.Email);

        return Ok(new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            Email = user.Email!,
            Username = user.UserName!,
            ExpiresAt = expiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        });
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        return Ok(new
        {
            user.Id,
            user.Email,
            user.UserName,
            user.FirstName,
            user.LastName,
            user.CreatedAt,
            user.LastLoginAt
        });
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var principal = _tokenService.GetPrincipalFromExpiredToken(model.AccessToken);
        if (principal == null)
        {
            return Unauthorized(new { message = "Invalid access token" });
        }

        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Invalid access token" });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.RefreshToken != model.RefreshToken)
        {
            return Unauthorized(new { message = "Invalid refresh token" });
        }

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return Unauthorized(new { message = "Refresh token expired" });
        }

        var newAccessToken = _tokenService.GenerateToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(60);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = refreshTokenExpiresAt;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {Email} refreshed token successfully", user.Email);

        return Ok(new AuthResponseDto
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            Email = user.Email!,
            Username = user.UserName!,
            ExpiresAt = expiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        });
    }

    [Authorize]
    [HttpPost("revoke")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Revoke()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {Email} revoked refresh token", user.Email);

        return Ok(new { message = "Refresh token revoked successfully" });
    }

    [Authorize]
    [HttpPost("2fa/enable")]
    [ProducesResponseType(typeof(Enable2FADto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Enable2FA()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        if (user.TwoFactorEnabled)
        {
            return BadRequest(new { message = "2FA is already enabled for this user" });
        }

        // Reset the authenticator key
        await _userManager.ResetAuthenticatorKeyAsync(user);
        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        
        if (string.IsNullOrEmpty(key))
        {
            return StatusCode(500, new { message = "Failed to generate authenticator key" });
        }

        var email = user.Email ?? user.UserName ?? "User";
        var qrCodeUri = GenerateQrCodeUri(email, key);

        _logger.LogInformation("User {Email} requested 2FA setup", user.Email);

        return Ok(new Enable2FADto
        {
            SharedKey = FormatKey(key),
            QrCodeUri = qrCodeUri,
            RecoveryCodes = null // Will be provided after verification
        });
    }

    [Authorize]
    [HttpPost("2fa/verify")]
    [ProducesResponseType(typeof(Enable2FADto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Verify2FA([FromBody] Verify2FADto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        // Verify the 2FA code
        var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (!is2faTokenValid)
        {
            _logger.LogWarning("User {Email} failed 2FA verification", user.Email);
            return BadRequest(new { message = "Invalid verification code" });
        }

        // Enable 2FA
        await _userManager.SetTwoFactorEnabledAsync(user, true);

        // Generate recovery codes
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        _logger.LogInformation("User {Email} enabled 2FA successfully", user.Email);

        return Ok(new Enable2FADto
        {
            SharedKey = null,
            QrCodeUri = null,
            RecoveryCodes = recoveryCodes?.ToArray()
        });
    }

    [Authorize]
    [HttpPost("2fa/disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Disable2FA([FromBody] Verify2FADto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        if (!user.TwoFactorEnabled)
        {
            return BadRequest(new { message = "2FA is not enabled for this user" });
        }

        // Verify the 2FA code before disabling
        var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (!is2faTokenValid)
        {
            _logger.LogWarning("User {Email} failed 2FA verification for disable", user.Email);
            return BadRequest(new { message = "Invalid verification code" });
        }

        // Disable 2FA
        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _userManager.ResetAuthenticatorKeyAsync(user);

        _logger.LogInformation("User {Email} disabled 2FA", user.Email);

        return Ok(new { message = "2FA disabled successfully" });
    }

    [HttpPost("login/2fa")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoginWith2FA([FromBody] LoginWith2FADto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // Check password first
        var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!passwordValid)
        {
            _logger.LogWarning("Failed login attempt for {Email}", model.Email);
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // Check if 2FA is enabled
        if (!user.TwoFactorEnabled)
        {
            return BadRequest(new { message = "2FA is not enabled for this user. Use regular login endpoint" });
        }

        // Verify 2FA code
        var verificationCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
        var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (!is2faTokenValid)
        {
            _logger.LogWarning("User {Email} failed 2FA verification during login", user.Email);
            return Unauthorized(new { message = "Invalid 2FA code" });
        }

        user.LastLoginAt = DateTime.UtcNow;
        
        var token = _tokenService.GenerateToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(60);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = refreshTokenExpiresAt;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {Email} logged in successfully with 2FA", model.Email);

        return Ok(new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            Email = user.Email!,
            Username = user.UserName!,
            ExpiresAt = expiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        });
    }

    [Authorize]
    [HttpGet("2fa/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get2FAStatus()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        var recoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

        return Ok(new
        {
            is2FAEnabled = user.TwoFactorEnabled,
            recoveryCodesLeft = recoveryCodesLeft
        });
    }

    [Authorize]
    [HttpPost("2fa/recovery-codes/regenerate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegenerateRecoveryCodes([FromBody] Verify2FADto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        if (!user.TwoFactorEnabled)
        {
            return BadRequest(new { message = "2FA must be enabled to regenerate recovery codes" });
        }

        // Verify the 2FA code before regenerating
        var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (!is2faTokenValid)
        {
            _logger.LogWarning("User {Email} failed 2FA verification for recovery code regeneration", user.Email);
            return BadRequest(new { message = "Invalid verification code" });
        }

        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        _logger.LogInformation("User {Email} regenerated recovery codes", user.Email);

        return Ok(new
        {
            recoveryCodes = recoveryCodes?.ToArray()
        });
    }

    private string GenerateQrCodeUri(string email, string key)
    {
        const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        return string.Format(AuthenticatorUriFormat, 
            Uri.EscapeDataString("WarrantyApp"),
            Uri.EscapeDataString(email),
            key);
    }

    private string FormatKey(string key)
    {
        // Format key as: XXXX XXXX XXXX XXXX XXXX XXXX XXXX XXXX
        var result = new System.Text.StringBuilder();
        int currentPosition = 0;
        while (currentPosition + 4 < key.Length)
        {
            result.Append(key.Substring(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < key.Length)
        {
            result.Append(key.Substring(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }

    [HttpGet("confirm-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            return BadRequest(new { message = "Invalid email confirmation request" });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return BadRequest(new { message = "User not found" });
        }

        if (user.EmailConfirmed)
        {
            return Ok(new { message = "Email already confirmed", emailConfirmed = true });
        }

        // Decode the token
        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Email confirmation failed for user {UserId}: {Errors}", 
                userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest(new { message = "Email confirmation failed", errors = result.Errors.Select(e => e.Description) });
        }

        _logger.LogInformation("User {Email} confirmed their email successfully", user.Email);

        return Ok(new { message = "Email confirmed successfully", emailConfirmed = true });
    }

    [HttpPost("resend-confirmation-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            // Don't reveal that the user doesn't exist
            return Ok(new { message = "If the email exists, a confirmation link has been sent" });
        }

        if (user.EmailConfirmed)
        {
            return BadRequest(new { message = "Email is already confirmed" });
        }

        // Generate new confirmation token
        var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmationToken));
        
        // Build confirmation URL
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
        var confirmationLink = $"{baseUrl}/api/Auth/confirm-email?userId={user.Id}&token={encodedToken}";
        
        // Send confirmation email
        try
        {
            var emailBody = GenerateConfirmationEmailBody(user.Email!, confirmationLink);
            await _emailService.SendEmailAsync(
                user.Email!,
                "Confirm Your Email - Warranty App",
                emailBody
            );
            _logger.LogInformation("Resent confirmation email to {Email}", model.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend confirmation email to {Email}", model.Email);
            return StatusCode(500, new { message = "Failed to send confirmation email" });
        }

        return Ok(new { message = "If the email exists, a confirmation link has been sent" });
    }

    [Authorize]
    [HttpGet("email-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetEmailStatus()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        return Ok(new
        {
            email = user.Email,
            emailConfirmed = user.EmailConfirmed
        });
    }

    private string GenerateConfirmationEmailBody(string email, string confirmationLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #777; }}
        .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 12px; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Welcome to Warranty App!</h1>
        </div>
        <div class=""content"">
            <h2>Confirm Your Email Address</h2>
            <p>Hi there,</p>
            <p>Thank you for registering with Warranty App. To complete your registration and start managing your warranties, please confirm your email address by clicking the button below:</p>
            <center>
                <a href=""{HtmlEncoder.Default.Encode(confirmationLink)}"" class=""button"">Confirm Email Address</a>
            </center>
            <p>Or copy and paste this link into your browser:</p>
            <p style=""word-break: break-all; color: #4CAF50;"">{HtmlEncoder.Default.Encode(confirmationLink)}</p>
            <div class=""warning"">
                <strong>Important:</strong> This confirmation link will expire in 24 hours for security reasons.
            </div>
            <p>If you didn't create an account with Warranty App, you can safely ignore this email.</p>
        </div>
        <div class=""footer"">
            <p>© 2025 Warranty App. All rights reserved.</p>
            <p>This is an automated email. Please do not reply to this message.</p>
        </div>
    </div>
</body>
</html>";
    }
}
