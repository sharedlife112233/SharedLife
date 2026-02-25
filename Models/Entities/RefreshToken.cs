using SharedLife.Utilities;

namespace SharedLife.Models.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = TimeHelper.Now;
    public DateTime? RevokedAt { get; set; }
    public bool IsExpired => TimeHelper.Now >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;
    
    // Foreign key
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
}
