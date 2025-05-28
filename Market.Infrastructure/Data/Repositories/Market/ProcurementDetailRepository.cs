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
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE ProcurementId = @ProcurementId";
        return await connection.QueryAsync<ProcurementDetail>(sql, new { ProcurementId = procurementId });
    }

    public async Task<IEnumerable<ProcurementDetail>> GetByProductAsync(long productId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE ProductId = @ProductId";
        return await connection.QueryAsync<ProcurementDetail>(sql, new { ProductId = productId });
    }

    public async Task<IEnumerable<ProcurementDetail>> GetProcurementDetailsWithProductsAsync(long procurementId)
    {
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