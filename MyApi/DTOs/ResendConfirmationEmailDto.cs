using System.ComponentModel.DataAnnotations;

namespace MyApi.DTOs;

public class ResendConfirmationEmailDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}
