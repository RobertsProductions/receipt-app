using System.ComponentModel.DataAnnotations;

namespace MyApi.DTOs;

public class Verify2FADto
{
    [Required]
    public required string Code { get; set; }
}
