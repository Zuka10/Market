using Market.Migration.Entities;

namespace Market.Migration.Abstractions;

public interface IMigrationRunner
{
    Task RunMigrationsAsync();
    Task RollbackToVersionAsync(string version);
    Task<IEnumerable<MigrationInfo>> GetMigrationHistoryAsync();
}