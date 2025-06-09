using Market.Migration.Abstractions;
using Market.Migration.CLI;
using Market.Migration.Core;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Market.Migration;

public static class DependecyInjection
{
    public static IServiceCollection AddMigrationRunner(this IServiceCollection services,
        string connectionString)
    {
        services.AddTransient<IDbConnection>(_ => new SqlConnection(connectionString));
        services.AddTransient<IMigrationRunner, MigrationRunner>();
        services.AddTransient<MigrationCliService>();

        return services;
    }
}