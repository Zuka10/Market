using Market.Domain.Abstractions.Repositories.Auth;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Abstractions;
using Market.Infrastructure.Data.Repositories.Auth;
using Market.Infrastructure.Data.Repositories.Market;
using System.Data;

namespace Market.Infrastructure.Data;

public class UnitOfWork(IDbConnectionFactory connectionFactory) : IUnitOfWork
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;
    private bool _disposed = false;

    // Auth repositories
    private IRoleRepository? _roles;
    private IUserRepository? _users;
    private IRefreshTokenRepository? _refreshTokens;

    // Market repositories
    private ILocationRepository? _locations;
    private IVendorRepository? _vendors;
    private ICategoryRepository? _categories;
    private IProductRepository? _products;
    private IVendorLocationRepository? _vendorLocations;
    private IDiscountRepository? _discounts;
    private IOrderRepository? _orders;
    private IOrderDetailRepository? _orderDetails;
    private IProcurementRepository? _procurements;
    private IProcurementDetailRepository? _procurementDetails;
    private IPaymentRepository? _payments;

    // Auth repositories properties
    public IRoleRepository Roles =>
        _roles ??= new RoleRepository(_connectionFactory);

    public IUserRepository Users =>
        _users ??= new UserRepository(_connectionFactory);

    public IRefreshTokenRepository RefreshTokens =>
        _refreshTokens ??= new RefreshTokenRepository(_connectionFactory);

    // Market repositories properties
    public ILocationRepository Locations =>
        _locations ??= new LocationRepository(_connectionFactory);

    public IVendorRepository Vendors =>
        _vendors ??= new VendorRepository(_connectionFactory);

    public ICategoryRepository Categories =>
        _categories ??= new CategoryRepository(_connectionFactory);

    public IProductRepository Products =>
        _products ??= new ProductRepository(_connectionFactory);

    public IVendorLocationRepository VendorLocations =>
        _vendorLocations ??= new VendorLocationRepository(_connectionFactory);

    public IDiscountRepository Discounts =>
        _discounts ??= new DiscountRepository(_connectionFactory);

    public IOrderRepository Orders =>
        _orders ??= new OrderRepository(_connectionFactory);

    public IOrderDetailRepository OrderDetails =>
        _orderDetails ??= new OrderDetailRepository(_connectionFactory);

    public IProcurementRepository Procurements =>
        _procurements ??= new ProcurementRepository(_connectionFactory);

    public IProcurementDetailRepository ProcurementDetails =>
        _procurementDetails ??= new ProcurementDetailRepository(_connectionFactory);

    public IPaymentRepository Payments =>
        _payments ??= new PaymentRepository(_connectionFactory);

    public async Task<int> SaveChangesAsync()
    {
        // In Dapper, changes are committed immediately unless in a transaction
        // This method is here for interface compatibility with EF-style UoW
        return await Task.FromResult(1);
    }

    public async Task BeginTransactionAsync()
    {
        _connection ??= await _connectionFactory.CreateConnectionAsync();

        _transaction ??= _connection.BeginTransaction();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            _transaction.Commit();
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            _transaction.Rollback();
            await DisposeTransactionAsync();
        }
    }

    private async Task DisposeTransactionAsync()
    {
        _transaction?.Dispose();
        _transaction = null;

        if (_connection != null)
        {
            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }

        await Task.CompletedTask;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}