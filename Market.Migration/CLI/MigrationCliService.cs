using Market.Migration.Abstractions;
using Microsoft.Extensions.Logging;

namespace Market.Migration.CLI;

public class MigrationCliService(IMigrationRunner migrationRunner, ILogger<MigrationCliService> logger)
{
    private readonly IMigrationRunner _migrationRunner = migrationRunner;
    private readonly ILogger<MigrationCliService> _logger = logger;

    public async Task<int> ExecuteAsync(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                await ShowHelpAsync();
                return 0;
            }

            var command = args[0].ToLower();

            return command switch
            {
                "migrate" => await RunMigrationsAsync(),
                "rollback" => await RollbackAsync(args),
                "status" => await ShowStatusAsync(),
                "help" => await ShowHelpAsync(),
                _ => await ShowHelpAsync()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while running migrations");
            return 1;
        }
    }

    private async Task<int> RunMigrationsAsync()
    {
        _logger.LogInformation("Starting database migrations...");
        await _migrationRunner.RunMigrationsAsync();
        _logger.LogInformation("Database migrations completed successfully.");
        return 0;
    }

    private async Task<int> RollbackAsync(string[] args)
    {
        if (args.Length < 2)
        {
            _logger.LogError("Rollback command requires a target version. Usage: rollback <version>");
            return 1;
        }

        var targetVersion = args[1];
        _logger.LogInformation("Rolling back to version {TargetVersion}...", targetVersion);
        await _migrationRunner.RollbackToVersionAsync(targetVersion);
        _logger.LogInformation("Rollback completed successfully.");
        return 0;
    }

    private async Task<int> ShowStatusAsync()
    {
        var history = await _migrationRunner.GetMigrationHistoryAsync();

        Console.WriteLine("Migration Status:");
        Console.WriteLine("================");

        foreach (var migration in history)
        {
            var status = migration.IsApplied ? "Applied" : "Pending";
            var appliedAt = migration.IsApplied ? migration.AppliedAt.ToString("yyyy-MM-dd HH:mm:ss") : "-";
            Console.WriteLine($"{migration.Version}: {migration.Description} [{status}] {appliedAt}");
        }

        return 0;
    }

    private static async Task<int> ShowHelpAsync()
    {
        Console.WriteLine("MarketAPI Migration Runner");
        Console.WriteLine("==========================");
        Console.WriteLine("Available commands:");
        Console.WriteLine("  migrate          - Run all pending migrations");
        Console.WriteLine("  rollback <ver>   - Rollback to specified version");
        Console.WriteLine("  status           - Show migration status");
        Console.WriteLine("  help             - Show this help message");

        return await Task.FromResult(0);
    }
}