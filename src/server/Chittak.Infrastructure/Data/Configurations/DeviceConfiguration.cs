namespace Chittak.Infrastructure.Data.Configurations;

public sealed class DeviceConfiguration : IEntityTypeConfiguration<DeviceEntity>
{
    public void Configure(EntityTypeBuilder<DeviceEntity> builder)
    {
        builder.ToTable("devices", t => t.HasCheckConstraint(
            "ck_devices_platform", "platform IN ('android', 'ios', 'linux', 'windows', 'macos')"));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

        // Stored as readable lowercase text ("android", "linux"), not as a number.
        builder.Property(x => x.Platform).HasConversion(
            v => v!.Value.ToString().ToLowerInvariant(), // EF never passes null to a converter
            v => Enum.Parse<PlatformType>(v, true));

        builder.HasIndex(x => new { x.UserId, x.RegistrationId }).IsUnique();
        builder.HasIndex(x => x.UserId).HasDatabaseName("idx_devices_user");
    }
}
