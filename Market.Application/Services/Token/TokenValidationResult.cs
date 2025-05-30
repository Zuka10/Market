namespace Market.Application.Services.Token;

public class TokenValidationResult
{
    public bool IsValid { get; set; }
    public long UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}