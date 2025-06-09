using Dapper;
using Market.Migration.Abstractions;
using Market.Migration.Entities;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Market.Migration.Core;

public class MigrationRunner(IDbConnection connection, ILogger<MigrationRunner> logger) : IMigrationRunner
{
    private readonly IDbConnection _connection = connection;
    private readonly ILogger<MigrationRunner> _logger = logger;
    private readonly List<IMigration> _migrations = LoadMigrations();

    public async Task RunMigrationsAsync()
    {
        await EnsureConnectionOpenAsync();
        await EnsureMigrationHistoryTableExistsAsync();

        var appliedMigrations = await GetAppliedMigrationsAsync();
        var pendingMigrations = _migrations
            .Where(m => !appliedMigrations.Contains(m.Version))
            .OrderBy(m => m.Version)
            .ToList();

        if (pendingMigrations.Count == 0)
        {
            _logger.LogInformation("No pending migrations found.");
            return;
        }

        foreach (var migration in pendingMigrations)
        {
            await ExecuteMigrationAsync(migration);
        }
    }

    public async Task RollbackToVersionAsync(string version)
    {
        await EnsureConnectionOpenAsync();
        var appliedMigrations = await GetAppliedMigrationsAsync();
        var migrationsToRollback = _migrations
            .Where(m => appliedMigrations.Contains(m.Version) &&
                       string.Compare(m.Version, version, StringComparison.Ordinal) > 0)
            .OrderByDescending(m => m.Version)
            .ToList();

        foreach (var migration in migrationsToRollback)
        {
            await RollbackMigrationAsync(migration);
        }
    }

    public async Task<IEnumerable<MigrationInfo>> GetMigrationHistoryAsync()
    {
        await EnsureConnectionOpenAsync();
        await EnsureMigrationHistoryTableExistsAsync();

        const string sql = @"
                SELECT Version, Description, AppliedAt 
                FROM __MigrationHistory 
                ORDER BY Version";

        var history = await _connection.QueryAsync<MigrationHistory>(sql);

        return _migrations.Select(m => new MigrationInfo(
            m.Version,
            m.Description,
            history.FirstOrDefault(h => h.Version == m.Version)?.AppliedAt ?? DateTime.MinValue,
            history.Any(h => h.Version == m.Version)
        ));
    }

    private async Task ExecuteMigrationAsync(IMigration migration)
    {
        await EnsureConnectionOpenAsync();
        using var transaction = _connection.BeginTransaction();
        try
        {
            _logger.LogInformation("Applying migration {MigrationVersion}: {MigrationDescription}", migration.Version, migration.Description);

            await migration.UpAsync(_connection, transaction);
            await RecordMigrationAsync(migration, transaction);

            transaction.Commit();
            _logger.LogInformation("Successfully applied migration {MigrationVersion}", migration.Version);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Failed to apply migration {MigrationVersion}", migration.Version);
            throw;
        }
    }

    private async Task RollbackMigrationAsync(IMigration migration)
    {
        await EnsureConnectionOpenAsync();
        using var transaction = _connection.BeginTransaction();
        try
        {
            _logger.LogInformation("Rolling back migration {MigrationVersion}: {MigrationDescription}", migration.Version, migration.Description);

            await migration.DownAsync(_connection, transaction);
            await RemoveMigrationRecordAsync(migration.Version, transaction);

            transaction.Commit();
            _logger.LogInformation("Successfully rolled back migration {MigrationVersion}", migration.Version);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Failed to rollback migration {MigrationVersion}", migration.Version);
            throw;
        }
    }

    private async Task EnsureMigrationHistoryTableExistsAsync()
    {
        await EnsureConnectionOpenAsync();
        const string sql = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='__MigrationHistory' AND xtype='U')
            CREATE TABLE __MigrationHistory (
                Version NVARCHAR(50) PRIMARY KEY,
                Description NVARCHAR(255) NOT NULL,
                AppliedAt DATETIME2 NOT NULL,
                CheckSum NVARCHAR(64) NOT NULL
            )";

        await _connection.ExecuteAsync(sql);
    }

    private async Task EnsureConnectionOpenAsync()
    {
        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }
    }

    private async Task<List<string>> GetAppliedMigrationsAsync()
    {
        await EnsureConnectionOpenAsync();
        const string sql = "SELECT Version FROM __MigrationHistory ORDER BY Version";
        return (await _connection.QueryAsync<string>(sql)).ToList();
    }

    private async Task RecordMigrationAsync(IMigration migration, IDbTransaction transaction)
    {
        const string sql = @"
                INSERT INTO __MigrationHistory (Version, Description, AppliedAt, CheckSum)
                VALUES (@Version, @Description, @AppliedAt, @CheckSum)";

        await _connection.ExecuteAsync(sql, new
        {
            Version = migration.Version,
            Description = migration.Description,
            AppliedAt = DateTime.UtcNow,
            CheckSum = CalculateCheckSum(migration)
        }, transaction);
    }

    private async Task RemoveMigrationRecordAsync(string version, IDbTransaction transaction)
    {
        const string sql = "DELETE FROM __MigrationHistory WHERE Version = @Version";
        await _connection.ExecuteAsync(sql, new { Version = version }, transaction);
    }

    private static List<IMigration> LoadMigrations()
    {
        return [.. Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IMigration).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<IMigration>()
            .OrderBy(m => m.Version)];
    }

    private static string CalculateCheckSum(IMigration migration)
    {
        var content = $"{migration.Version}|{migration.Description}|{migration.GetType().FullName}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hash);
    }
}