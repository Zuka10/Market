using Market.Migration;
using Market.Migration.CLI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DotNetEnv;

// Load environment variables from API project
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "Market.Migration.Tool", ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
    Console.WriteLine($"✅ Loaded environment variables from: {envPath}");
}
else
{
    Console.WriteLine($"⚠️  .env file not found at: {envPath}");
    Console.WriteLine("Trying to load from current directory...");
    if (File.Exists(".env"))
    {
        Env.Load();
        Console.WriteLine("✅ Loaded .env from current directory");
    }
    else
    {
        Console.WriteLine("❌ No .env file found");
    }
}

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("❌ Error: Connection string not found.");
    Console.WriteLine("Please ensure .env file exists with ConnectionStrings__DefaultConnection");
    Console.WriteLine("Expected location: Market.API/.env");
    return 1;
}

Console.WriteLine($"✅ Using connection string: {connectionString[..50]}...");

// Setup DI
var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});
services.AddMigrationRunner(connectionString);

using var serviceProvider = services.BuildServiceProvider();

try
{
    Console.WriteLine("🚀 Starting Migration Tool...");
    var cliService = serviceProvider.GetRequiredService<MigrationCliService>();
    var exitCode = await cliService.ExecuteAsync(args);

    Console.WriteLine($"✅ Migration tool completed with exit code: {exitCode}");
    return exitCode;
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Migration CLI failed: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    return 1;
}