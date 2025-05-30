using Dapper;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Entities.Market;
using Market.Infrastructure.Constants;

namespace Market.Infrastructure.Data.Repositories.Market;

public class ProcurementDetailRepository(IDbConnectionFactory connectionFactory) :
    GenericRepository<ProcurementDetail>(connectionFactory, DatabaseConstants.Tables.Market.ProcurementDetail, DatabaseConstants.Schemas.Market),
    IProcurementDetailRepository
{
    public async Task<IEnumerable<ProcurementDetail>> GetByProcurementAsync(long procurementId)
    {
        using var procurementConnection = await _connectionFactory.CreateConnectionAsync();
        var procurementCheckSql = "SELECT COUNT(1) FROM market.Procurement WHERE Id = @ProcurementId";
        var procurementExists = await procurementConnection.QuerySingleAsync<int>(procurementCheckSql, new { ProcurementId = procurementId });
        if (procurementExists == 0)
        {
            throw new KeyNotFoundException($"Procurement with ID '{procurementId}' was not found.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE ProcurementId = @ProcurementId";
        return await connection.QueryAsync<ProcurementDetail>(sql, new { ProcurementId = procurementId });
    }

    public async Task<IEnumerable<ProcurementDetail>> GetByProductAsync(long productId)
    {
        using var productConnection = await _connectionFactory.CreateConnectionAsync();
        var productCheckSql = "SELECT COUNT(1) FROM market.Product WHERE Id = @ProductId";
        var productExists = await productConnection.QuerySingleAsync<int>(productCheckSql, new { ProductId = productId });
        if (productExists == 0)
        {
            throw new KeyNotFoundException($"Product with ID '{productId}' was not found.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE ProductId = @ProductId";
        return await connection.QueryAsync<ProcurementDetail>(sql, new { ProductId = productId });
    }

    public async Task<IEnumerable<ProcurementDetail>> GetProcurementDetailsWithProductsAsync(long procurementId)
    {
        using var procurementConnection = await _connectionFactory.CreateConnectionAsync();
        var procurementCheckSql = "SELECT COUNT(1) FROM market.Procurement WHERE Id = @ProcurementId";
        var procurementExists = await procurementConnection.QuerySingleAsync<int>(procurementCheckSql, new { ProcurementId = procurementId });
        if (procurementExists == 0)
        {
            throw new KeyNotFoundException($"Procurement with ID '{procurementId}' was not found.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
                SELECT pd.*, p.Id, p.Name, p.Description, p.Unit, p.Price
                FROM market.ProcurementDetail pd
                INNER JOIN market.Product p ON pd.ProductId = p.Id
                WHERE pd.ProcurementId = @ProcurementId";
        var result = await connection.QueryAsync<ProcurementDetail, Product, ProcurementDetail>(
            sql,
            (procurementDetail, product) =>
            {
                procurementDetail.Product = product;
                return procurementDetail;
            },
            new { ProcurementId = procurementId },
            splitOn: "Id");
        return result;
    }

    public override async Task UpdateAsync(ProcurementDetail entity)
    {
        await ValidateForeignKeys(entity);

        ValidateProcurementDetailRules(entity);

        await base.UpdateAsync(entity);
    }

    public override async Task<ProcurementDetail> AddAsync(ProcurementDetail entity)
    {
        await ValidateForeignKeys(entity);

        ValidateProcurementDetailRules(entity);

        await ValidateProductAvailability(entity);

        return await base.AddAsync(entity);
    }

    private async Task ValidateForeignKeys(ProcurementDetail entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var procurementCheckSql = "SELECT COUNT(1) FROM market.Procurement WHERE Id = @ProcurementId";
        var procurementExists = await connection.QuerySingleAsync<int>(procurementCheckSql, new { ProcurementId = entity.ProcurementId });
        if (procurementExists == 0)
        {
            throw new ArgumentException($"Procurement with ID '{entity.ProcurementId}' does not exist.");
        }

        var productCheckSql = "SELECT COUNT(1) FROM market.Product WHERE Id = @ProductId";
        var productExists = await connection.QuerySingleAsync<int>(productCheckSql, new { ProductId = entity.ProductId });
        if (productExists == 0)
        {
            throw new ArgumentException($"Product with ID '{entity.ProductId}' does not exist.");
        }
    }

    private static void ValidateProcurementDetailRules(ProcurementDetail entity)
    {
        if (entity.Quantity <= 0)
        {
            throw new ArgumentException("Procurement detail quantity must be greater than zero.");
        }

        if (entity.PurchasePrice < 0)
        {
            throw new ArgumentException("Procurement detail purchase price cannot be negative.");
        }

        if (entity.LineTotal < 0)
        {
            throw new ArgumentException("Procurement detail line total cannot be negative.");
        }

        var expectedLineTotal = entity.Quantity * entity.PurchasePrice;
        if (Math.Abs(entity.LineTotal - expectedLineTotal) > 0.01m)
        {
            throw new ArgumentException($"Line total ({entity.LineTotal:C}) does not match quantity ({entity.Quantity}) × purchase price ({entity.PurchasePrice:C}) = {expectedLineTotal:C}.");
        }

        if (entity.PurchasePrice > 10000) // Reasonable upper limit per unit
        {
            throw new ArgumentException("Purchase price per unit cannot exceed $10,000.");
        }
    }

    private async Task ValidateProductAvailability(ProcurementDetail entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var duplicateCheckSql = $"SELECT COUNT(1) FROM {FullTableName} WHERE ProcurementId = @ProcurementId AND ProductId = @ProductId";
        var duplicateExists = await connection.QuerySingleAsync<int>(duplicateCheckSql,
            new { entity.ProcurementId, entity.ProductId });

        if (duplicateExists > 0)
        {
            throw new ArgumentException($"Product with ID '{entity.ProductId}' is already in procurement '{entity.ProcurementId}'. Update the existing procurement detail instead of adding a new one.");
        }

        var productStatusSql = "SELECT IsAvailable FROM market.Product WHERE Id = @ProductId";
        var isAvailable = await connection.QuerySingleAsync<bool>(productStatusSql, new { entity.ProductId });

        if (!isAvailable)
        {
            throw new ArgumentException($"Product with ID '{entity.ProductId}' is not available for procurement.");
        }
    }

    protected override string GenerateInsertQuery()
    {
        return @"
                INSERT INTO market.ProcurementDetail (ProcurementId, ProductId, PurchasePrice, Quantity, LineTotal)
                VALUES (@ProcurementId, @ProductId, @PurchasePrice, @Quantity, @LineTotal);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";
    }

    protected override string GenerateUpdateQuery()
    {
        return @"
                UPDATE market.ProcurementDetail 
                SET ProcurementId = @ProcurementId, 
                    ProductId = @ProductId, 
                    PurchasePrice = @PurchasePrice, 
                    Quantity = @Quantity, 
                    LineTotal = @LineTotal
                WHERE Id = @Id";
    }
}