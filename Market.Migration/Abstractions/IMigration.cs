using System.Data;

namespace Market.Migration.Abstractions;

public interface IMigration
{
    string Version { get; }
    string Description { get; }
    Task UpAsync(IDbConnection connection, IDbTransaction? transaction);
    Task DownAsync(IDbConnection connection, IDbTransaction? transaction);
}