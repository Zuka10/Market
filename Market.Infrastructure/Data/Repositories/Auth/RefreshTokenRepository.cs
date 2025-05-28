using Dapper;
using Market.Domain.Abstractions.Repositories.Auth;
using Market.Domain.Entities.Auth;
using Market.Infrastructure.Constants;

namespace Market.Infrastructure.Data.Repositories.Auth;

public class RefreshTokenRepository(IDbConnectionFactory connectionFactory) :
    GenericRepository<RefreshToken>(connectionFactory, DatabaseConstants.Tables.Auth.RefreshToken, DatabaseConstants.Schemas.Auth),
    IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {FullTableName} WHERE Token = @Token";
        return await connection.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { Token = token });
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserAsync(long userId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                SELECT * FROM {FullTableName} 
                WHERE UserId = @UserId 
                  AND IsRevoked = 0 
                  AND IsUsed = 0 
                  AND ExpiresAt > GETUTCDATE()";
        return await connection.QueryAsync<RefreshToken>(sql, new { UserId = userId });
    }

    public async Task RevokeTokenAsync(string token)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                UPDATE {FullTableName} 
                SET IsRevoked = 1, RevokedAt = GETUTCDATE() 
                WHERE Token = @Token";
        await connection.ExecuteAsync(sql, new { Token = token });
    }

    public async Task RevokeUserTokensAsync(long userId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                UPDATE {FullTableName} 
                SET IsRevoked = 1, RevokedAt = GETUTCDATE() 
                WHERE UserId = @UserId AND IsRevoked = 0";
        await connection.ExecuteAsync(sql, new { UserId = userId });
    }

    public async Task CleanupExpiredTokensAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $@"
                DELETE FROM {FullTableName} 
                WHERE ExpiresAt < GETUTCDATE() 
                   OR (IsRevoked = 1 AND RevokedAt < DATEADD(day, -30, GETUTCDATE()))";
        await connection.ExecuteAsync(sql);
    }

    protected override string GenerateInsertQuery()
    {
        return @"
                INSERT INTO auth.RefreshToken (UserId, Token, ExpiresAt, CreatedAt, IsRevoked, IsUsed)
                VALUES (@UserId, @Token, @ExpiresAt, @CreatedAt, @IsRevoked, @IsUsed);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";
    }

    protected override string GenerateUpdateQuery()
    {
        return @"
                UPDATE auth.RefreshToken 
                SET UserId = @UserId, 
                    Token = @Token, 
                    ExpiresAt = @ExpiresAt, 
                    RevokedAt = @RevokedAt, 
                    IsRevoked = @IsRevoked, 
                    IsUsed = @IsUsed
                WHERE Id = @Id";
    }
}