using System.ComponentModel.DataAnnotations;

namespace MyApi.DTOs;

public class LoginWith2FADto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }

    [Required]
    public required string TwoFactorCode { get; set; }
}
