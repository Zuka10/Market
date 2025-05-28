using Market.Domain.Abstractions.Repositories.Auth;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Abstractions;
using Market.Infrastructure.Data.Repositories.Auth;
using Market.Infrastructure.Data.Repositories.Market;
using Market.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Market.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register connection factory
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddSingleton<IDbConnectionFactory>(provider =>
            new SqlConnectionFactory(connectionString!));

        // Register Auth repositories
        AddAuthRepositories(services);

        // Register Market repositories
        AddMarketRepositories(services);

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static void AddMarketRepositories(IServiceCollection services)
    {
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IVendorLocationRepository, VendorLocationRepository>();
        services.AddScoped<IDiscountRepository, DiscountRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
        services.AddScoped<IProcurementRepository, ProcurementRepository>();
        services.AddScoped<IProcurementDetailRepository, ProcurementDetailRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
    }

    private static void AddAuthRepositories(this IServiceCollection services)
    {
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    }
}