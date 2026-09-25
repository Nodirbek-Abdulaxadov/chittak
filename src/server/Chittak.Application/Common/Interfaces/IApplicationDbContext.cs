namespace Chittak.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<AuthSessionEntity> AuthSessions { get; }
    DbSet<DeviceEntity> Devices { get; }
    DbSet<DeviceIdentityKeyEntity> DeviceIdentityKeys { get; }
    DbSet<MessageQueueEntity> MessageQueues { get; }
    DbSet<OneTimePrekeyEntity> OneTimePrekeys { get; }
    DbSet<PhoneVerificationEntity> PhoneVerifications { get; }
    DbSet<SignedPrekeyEntity> SignedPrekeys { get; }
    DbSet<UserEntity> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}