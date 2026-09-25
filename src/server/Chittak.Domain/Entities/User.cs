namespace Chittak.Domain.Entities;

public class UserEntity : BaseEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Phone { get; set; } = string.Empty;
    public string PhoneHash { get; set; } = string.Empty;
    public bool Discoverable { get; set; } = true;
    public string? DisplayName { get; set; }
    public ICollection<DeviceEntity> Devices { get; set; } = [];
}
