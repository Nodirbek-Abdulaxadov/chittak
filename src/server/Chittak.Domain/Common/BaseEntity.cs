namespace Chittak.Domain.Common;

// Every table in chittak-db.sql has created_at and nothing else in common:
// key types differ (uuid vs bigint identity vs device_id) and there is no updated_at.
public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
