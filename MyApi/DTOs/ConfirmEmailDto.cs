using System.ComponentModel.DataAnnotations;

namespace MyApi.DTOs;

public class ConfirmEmailDto
{
    [Required]
    public required string UserId { get; set; }

    [Required]
    public required string Token { get; set; }
}
