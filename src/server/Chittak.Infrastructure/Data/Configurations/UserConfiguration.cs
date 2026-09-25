namespace Chittak.Infrastructure.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        // E.164 with '+' is up to 16 characters, so a length limit is not enough: the CHECK is the rule.
        builder.ToTable("users", t => t.HasCheckConstraint("ck_users_phone_e164", @"phone ~ '^\+[1-9][0-9]{6,14}$'"));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

        // Sentinel = true: EF omits the column only when the value is true (DB default gives the same),
        // so an explicit false is still sent. Without it EF treats false as "not set" and the DB writes true.
        builder.Property(x => x.Discoverable).HasDefaultValue(true).HasSentinel(true);

        builder.HasIndex(x => x.Phone).IsUnique();
        builder.HasIndex(x => x.PhoneHash).IsUnique();

        builder.HasMany(x => x.Devices)
               .WithOne(x => x.User)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
