using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeInOneMinute.Domain.Model;

[Table("users", Schema = "auth")]
public class User
{
    [Key]
    public long UserId { get; set; }

    public Guid GlobalUserId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

    [Required]
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [Column("refresh_token_hash")]
    public byte[]? RefreshTokenHash { get; set; }

    [Column("refresh_token_salt")]
    public byte[]? RefreshTokenSalt { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    [MaxLength(255)]
    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpiryTime { get; set; }

    // Navigation property
    public virtual UserProfile? UserProfile { get; set; }
}
