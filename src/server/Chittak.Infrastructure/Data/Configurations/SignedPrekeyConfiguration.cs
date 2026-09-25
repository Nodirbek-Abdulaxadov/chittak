namespace Chittak.Infrastructure.Data.Configurations;

public sealed class SignedPrekeyConfiguration : IEntityTypeConfiguration<SignedPrekeyEntity>
{
    public void Configure(EntityTypeBuilder<SignedPrekeyEntity> builder)
    {
        builder.ToTable("signed_prekeys", t =>
        {
            t.HasCheckConstraint("ck_signed_prekeys_public_key_length", "octet_length(public_key) = 32");
            t.HasCheckConstraint("ck_signed_prekeys_signature_length", "octet_length(signature) = 64");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(x => new { x.DeviceId, x.KeyId }).IsUnique();
        builder.HasIndex(x => new { x.DeviceId, x.CreatedAt })
               .IsDescending(false, true)
               .HasDatabaseName("idx_signed_prekeys_device");

        builder.HasOne(x => x.Device)
               .WithMany()
               .HasForeignKey(x => x.DeviceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
