namespace MyApi.DTOs;

public class Enable2FADto
{
    public string? SharedKey { get; set; }
    public string? QrCodeUri { get; set; }
    public string[]? RecoveryCodes { get; set; }
}
