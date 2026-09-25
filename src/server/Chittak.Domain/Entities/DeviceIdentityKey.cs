namespace Chittak.Domain.Entities;

// One row per device, keyed by the device itself (no separate id).
public class DeviceIdentityKeyEntity : BaseEntity
{
    public Guid DeviceId { get; set; }
    public DeviceEntity? Device { get; set; }

    // 64 bytes: Ed25519 public (32) || X25519 public (32)
    public byte[] IdentityKey { get; set; } = [];
}
