namespace ECommerceAPI.Models;

public class PasswordResetToken
{
    public int       Id        { get; set; }
    public int       UserId    { get; set; }
    public string    Token     { get; set; } = string.Empty;
    public DateTime  ExpiresAt { get; set; }
    public DateTime? UsedAt    { get; set; }
    public DateTime  CreatedAt { get; set; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsUsed    => UsedAt.HasValue;
    public bool IsValid   => !IsExpired && !IsUsed;
}
