namespace Chittak.Domain.Entities;

public class PhoneVerificationEntity : BaseEntity
{
    public long Id { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(5);
    public int Attempts { get; set; }
    public DateTime? ConsumedAt { get; set; }
}
