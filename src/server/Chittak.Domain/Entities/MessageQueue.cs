namespace Chittak.Domain.Entities;

public class MessageQueueEntity : BaseEntity
{
    public long Id { get; set; }
    public Guid RecipientDeviceId { get; set; }
    public DeviceEntity? RecipientDevice { get; set; }

    // Routing only, deliberately NOT a foreign key: queued messages must survive
    // the sender device being deleted, and sealed sender will leave it null.
    public Guid? SenderDeviceId { get; set; }

    public Guid ClientMessageId { get; set; }
    public byte[] Envelope { get; set; } = [];
    public EnvelopeType EnvelopeType { get; set; } = EnvelopeType.Prekey;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);
}
