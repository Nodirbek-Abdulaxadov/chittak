namespace Chittak.Infrastructure.Data.Configurations;

public sealed class OneTimePrekeyConfiguration : IEntityTypeConfiguration<OneTimePrekeyEntity>
{
    public void Configure(EntityTypeBuilder<OneTimePrekeyEntity> builder)
    {
        builder.ToTable("one_time_prekeys", t => t.HasCheckConstraint(
            "ck_one_time_prekeys_public_key_length", "octet_length(public_key) = 32"));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(x => new { x.DeviceId, x.KeyId }).IsUnique();
        builder.HasIndex(x => x.DeviceId).HasDatabaseName("idx_otk_device");

        builder.HasOne(x => x.Device)
               .WithMany()
               .HasForeignKey(x => x.DeviceId)
               .OnDelete(DeleteBehavior.Cascade);

        // Handing out an OTK is NOT done through EF (Remove + SaveChanges is not atomic under
        // concurrency): use the single DELETE ... FOR UPDATE SKIP LOCKED ... RETURNING from chittak-db.sql.
    }
}
