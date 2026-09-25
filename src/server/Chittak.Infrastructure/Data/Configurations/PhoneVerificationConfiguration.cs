namespace Chittak.Infrastructure.Data.Configurations;

public sealed class PhoneVerificationConfiguration : IEntityTypeConfiguration<PhoneVerificationEntity>
{
    public void Configure(EntityTypeBuilder<PhoneVerificationEntity> builder)
    {
        builder.ToTable("phone_verifications", t => t.HasCheckConstraint(
            "ck_phone_verifications_attempts", "attempts <= 5"));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.Attempts).HasDefaultValue(0);

        // Rate limits (3 per 10 min, 10 per day) are counted through this index.
        builder.HasIndex(x => new { x.Phone, x.CreatedAt })
               .IsDescending(false, true)
               .HasDatabaseName("idx_phone_verif");
    }
}
