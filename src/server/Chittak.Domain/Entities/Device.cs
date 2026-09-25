namespace Chittak.Domain.Entities;

public class DeviceEntity : BaseEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public UserEntity? User { get; set; }
    public int RegistrationId { get; set; }
    public string? Name { get; set; }
    public PlatformType? Platform { get; set; }
    public string? PushToken { get; set; }
    public DateTime? LastSeenAt { get; set; }
}
