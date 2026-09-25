namespace Chittak.Infrastructure.Data.Configurations;

public sealed class AuthSessionConfiguration : IEntityTypeConfiguration<AuthSessionEntity>
{
    public void Configure(EntityTypeBuilder<AuthSessionEntity> builder)
    {
        builder.ToTable("auth_sessions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(x => x.DeviceId).HasDatabaseName("idx_auth_device");

        builder.HasOne(x => x.Device)
               .WithMany()
               .HasForeignKey(x => x.DeviceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
