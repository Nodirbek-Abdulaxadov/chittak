using Chittak.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Chittak.IntegrationTests;

// S0-05: chittak-db.sql sxema kontrakti. EF migratsiyasi xuddi shu ustunlar, FK'lar, CHECK'lar va
// indekslarni yaratishi shart. Constraint/indeks NOMLARI farq qilishi mumkin (EF: ck_*/ix_*) — shuning
// uchun nomlar emas, ta'riflar solishtiriladi.
public sealed class SchemaContractTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public Task InitializeAsync() => _postgres.StartAsync();

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task MigrationMatchesSqlContract()
    {
        await ExecuteAsync(_postgres.GetConnectionString(), "CREATE DATABASE contract");
        await ExecuteAsync(_postgres.GetConnectionString(), "CREATE DATABASE ef");
        string contractDb = WithDatabase("contract");
        string efDb = WithDatabase("ef");

        // (a) kontrakt: SQL faylning o'zi
        await ExecuteAsync(contractDb, await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "chittak-db.sql")));

        // (b) EF: ilova ishlatadigan aynan shu migratsiyalar
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(efDb)
            .UseSnakeCaseNamingConvention()
            .Options;
        await using (var db = new ApplicationDbContext(options))
        {
            await db.Database.MigrateAsync();
        }

        var contract = await DescribeAsync(contractDb);
        var ef = await DescribeAsync(efDb);

        // Himoya: so'rov xato bo'lsa ikkala ro'yxat ham bo'sh chiqib, "mos" deb ko'rinadi.
        contract.Should().HaveCountGreaterThan(50, "describe so'rovi haqiqiy sxemani qaytarishi kerak");
        ef.Should().Equal(contract);
    }

    private string WithDatabase(string database) =>
        new NpgsqlConnectionStringBuilder(_postgres.GetConnectionString()) { Database = database }.ConnectionString;

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<List<string>> DescribeAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(DescribeSql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var rows = new List<string>();
        while (await reader.ReadAsync())
        {
            rows.Add(reader.GetString(0));
        }
        return rows;
    }

    // Har qator = sxemaning bitta faktı; ikkala bazada bir xil tartibda.
    private const string DescribeSql = """
        -- ustunlar: tur, nullable, default, identity
        SELECT 'COL ' || table_name || '.' || column_name || ' ' || data_type || ' null=' || is_nullable
               || ' default=' || coalesce(regexp_replace(column_default, '^\((.*)\)$', '\1'), '-')
               || ' identity=' || coalesce(identity_generation, '-')
        FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name <> '__EFMigrationsHistory'
        UNION ALL
        -- FK'lar va ON DELETE qoidasi
        SELECT 'FK  ' || c.conrelid::regclass || '(' || a.attname || ') -> ' || c.confrelid::regclass
               || ' on delete ' || c.confdeltype::text
        FROM pg_constraint c
        JOIN pg_attribute a ON a.attrelid = c.conrelid AND a.attnum = ANY (c.conkey)
        WHERE c.contype = 'f' AND c.connamespace = 'public'::regnamespace
        UNION ALL
        -- CHECK'lar: faqat ta'rif
        SELECT 'CHK ' || conrelid::regclass || ' ' || pg_get_constraintdef(oid)
        FROM pg_constraint
        WHERE contype = 'c' AND connamespace = 'public'::regnamespace
        UNION ALL
        -- barcha indekslar (PK va UNIQUE ham indeks): ustunlar, tartib, unique
        SELECT 'IDX ' || t.relname || ' ' || CASE WHEN i.indisunique THEN 'unique ' ELSE '' END
               || regexp_replace(pg_get_indexdef(i.indexrelid), '^.* USING btree ', '')
        FROM pg_index i
        JOIN pg_class t ON t.oid = i.indrelid
        WHERE t.relnamespace = 'public'::regnamespace AND t.relname <> '__EFMigrationsHistory'
        ORDER BY 1
        """;
}
