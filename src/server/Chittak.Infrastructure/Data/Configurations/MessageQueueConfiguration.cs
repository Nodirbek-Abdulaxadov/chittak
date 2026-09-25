namespace Chittak.Infrastructure.Data.Configurations;

public sealed class MessageQueueConfiguration : IEntityTypeConfiguration<MessageQueueEntity>
{
    public void Configure(EntityTypeBuilder<MessageQueueEntity> builder)
    {
        builder.ToTable("message_queue", t => t.HasCheckConstraint(
            "ck_message_queue_envelope_type", "envelope_type IN (1, 2, 3)"));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

        // Idempotency: a retried POST with the same client_message_id is rejected here.
        builder.HasIndex(x => new { x.RecipientDeviceId, x.ClientMessageId }).IsUnique();
        builder.HasIndex(x => new { x.RecipientDeviceId, x.Id }).HasDatabaseName("idx_queue_recipient");
        builder.HasIndex(x => x.ExpiresAt).HasDatabaseName("idx_queue_expiry");

        builder.HasOne(x => x.RecipientDevice)
               .WithMany()
               .HasForeignKey(x => x.RecipientDeviceId)
               .OnDelete(DeleteBehavior.Cascade);

        // SenderDeviceId has no navigation on purpose, so EF creates no FK for it.
    }
}
