namespace Chittak.Infrastructure.Data.Configurations;

public sealed class DeviceIdentityKeyConfiguration : IEntityTypeConfiguration<DeviceIdentityKeyEntity>
{
    public void Configure(EntityTypeBuilder<DeviceIdentityKeyEntity> builder)
    {
        builder.ToTable("device_identity_keys", t => t.HasCheckConstraint(
            "ck_device_identity_keys_length", "octet_length(identity_key) = 64"));

        builder.HasKey(x => x.DeviceId);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

        builder.HasOne(x => x.Device)
               .WithOne()
               .HasForeignKey<DeviceIdentityKeyEntity>(x => x.DeviceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
