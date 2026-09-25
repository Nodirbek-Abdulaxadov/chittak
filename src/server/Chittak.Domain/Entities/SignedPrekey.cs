namespace Chittak.Domain.Entities;

public class SignedPrekeyEntity : BaseEntity
{
    public long Id { get; set; }
    public Guid DeviceId { get; set; }
    public DeviceEntity? Device { get; set; }
    public int KeyId { get; set; }
    public byte[] PublicKey { get; set; } = [];
    public byte[] Signature { get; set; } = [];
}
