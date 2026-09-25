namespace Chittak.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions options) 
    : DbContext(options), IApplicationDbContext
{
    public DbSet<AuthSessionEntity> AuthSessions => Set<AuthSessionEntity>();
    public DbSet<DeviceEntity> Devices => Set<DeviceEntity>();
    public DbSet<DeviceIdentityKeyEntity> DeviceIdentityKeys => Set<DeviceIdentityKeyEntity>();
    public DbSet<MessageQueueEntity> MessageQueues => Set<MessageQueueEntity>();
    public DbSet<OneTimePrekeyEntity> OneTimePrekeys => Set<OneTimePrekeyEntity>();
    public DbSet<PhoneVerificationEntity> PhoneVerifications => Set<PhoneVerificationEntity>();
    public DbSet<SignedPrekeyEntity> SignedPrekeys => Set<SignedPrekeyEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
