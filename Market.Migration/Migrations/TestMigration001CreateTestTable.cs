using Market.Migration.Abstractions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Market.Migration.Migrations;

public class TestMigration001CreateTestTable : IMigration
{
    public string Version => "20250108_001";
    public string Description => "Create Test Table for Migration Testing";

    public async Task UpAsync(IDbConnection connection, IDbTransaction? transaction)
    {
        // Create test table
        var createTableSql = @"
            CREATE TABLE TestMigrationTable (
                Id BIGINT IDENTITY(1,1) PRIMARY KEY,
                Name NVARCHAR(100) NOT NULL,
                Description NVARCHAR(500) NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                IsActive BIT NOT NULL DEFAULT 1
            );";

        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = createTableSql;
        await ExecuteNonQueryAsync(command);

        // Create index
        var createIndexSql = @"
            CREATE INDEX IX_TestMigrationTable_Name 
            ON TestMigrationTable(Name);";

        command.CommandText = createIndexSql;
        await ExecuteNonQueryAsync(command);

        // Insert test data
        var insertDataSql = @"
            INSERT INTO TestMigrationTable (Name, Description) VALUES 
            ('Test Item 1', 'This is a test migration item'),
            ('Test Item 2', 'Another test item for verification'),
            ('Test Item 3', 'Third test item for testing rollback');";

        command.CommandText = insertDataSql;
        await ExecuteNonQueryAsync(command);

        Console.WriteLine($"✅ Migration {Version} applied successfully - Created TestMigrationTable with 3 test records");
    }

    public async Task DownAsync(IDbConnection connection, IDbTransaction? transaction)
    {
        // Drop the test table (this will also drop the index)
        var dropTableSql = @"
            IF OBJECT_ID('TestMigrationTable', 'U') IS NOT NULL
                DROP TABLE TestMigrationTable;";

        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = dropTableSql;
        await ExecuteNonQueryAsync(command);

        Console.WriteLine($"✅ Migration {Version} rolled back successfully - Dropped TestMigrationTable");
    }

    private static async Task ExecuteNonQueryAsync(IDbCommand command)
    {
        if (command is SqlCommand sqlCommand)
        {
            await sqlCommand.ExecuteNonQueryAsync();
        }
        else
        {
            // Fallback for other IDbCommand implementations
            await Task.Run(() => command.ExecuteNonQuery());
        }
    }
}
