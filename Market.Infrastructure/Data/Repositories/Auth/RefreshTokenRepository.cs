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
        var refreshToken = await connection.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { Token = token });

        return refreshToken ?? throw new KeyNotFoundException($"Refresh token '{token}' was not found.");
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
        var checkSql = $"SELECT COUNT(1) FROM {FullTableName} WHERE Token = @Token";
        var tokenExists = await connection.QuerySingleAsync<int>(checkSql, new { Token = token });

        if (tokenExists == 0)
        {
            throw new KeyNotFoundException($"Refresh token '{token}' was not found and cannot be revoked.");
        }

        var sql = $@"
                UPDATE {FullTableName} 
                SET IsRevoked = 1, RevokedAt = GETUTCDATE() 
                WHERE Token = @Token";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Token = token });

        if (rowsAffected == 0)
        {
            throw new KeyNotFoundException($"Refresh token '{token}' was not found and cannot be revoked.");
        }
    }

    public async Task RevokeUserTokensAsync(long userId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var checkSql = $"SELECT COUNT(1) FROM {FullTableName} WHERE UserId = @UserId AND IsRevoked = 0";
        var activeTokensCount = await connection.QuerySingleAsync<int>(checkSql, new { UserId = userId });

        if (activeTokensCount == 0)
        {
            throw new KeyNotFoundException($"No active refresh tokens found for user with ID '{userId}'.");
        }

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

    public override async Task UpdateAsync(RefreshToken entity)
    {
        if (!await ExistsAsync(entity.Id))
        {
            throw new KeyNotFoundException($"Refresh token with ID '{entity.Id}' was not found and cannot be updated.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var tokenSql = $"SELECT IsUsed, IsRevoked FROM {FullTableName} WHERE Id = @Id";
        var tokenStatus = await connection.QueryFirstOrDefaultAsync<dynamic>(tokenSql, new { entity.Id });

        if (tokenStatus != null && (bool)tokenStatus!.IsUsed)
        {
            throw new InvalidOperationException($"Cannot update refresh token with ID '{entity.Id}' because it has already been used.");
        }

        await base.UpdateAsync(entity);
    }

    public override async Task<RefreshToken> AddAsync(RefreshToken entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var tokenCheckSql = $"SELECT COUNT(1) FROM {FullTableName} WHERE Token = @Token";
        var tokenExists = await connection.QuerySingleAsync<int>(tokenCheckSql, new { Token = entity.Token });
        if (tokenExists > 0)
        {
            throw new ArgumentException($"Refresh token '{entity.Token}' already exists.");
        }

        if (entity.ExpiresAt <= DateTime.UtcNow)
        {
            throw new ArgumentException("Refresh token expiration date must be in the future.");
        }

        return await base.AddAsync(entity);
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