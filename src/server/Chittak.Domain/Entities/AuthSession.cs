namespace Chittak.Domain.Entities;

public class AuthSessionEntity : BaseEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid DeviceId { get; set; }
    public DeviceEntity? Device { get; set; }
    public string RefreshTokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(30);
    public DateTime? RevokedAt { get; set; }
}
