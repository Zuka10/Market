namespace Market.Application.DTOs.Auth;

public class RefreshTokenDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsUsed { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
}