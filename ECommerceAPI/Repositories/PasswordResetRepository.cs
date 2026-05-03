using ECommerceAPI.Data;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;

namespace ECommerceAPI.Repositories;

public class PasswordResetRepository : IPasswordResetRepository
{
    private readonly DbConnectionFactory _db;

    public PasswordResetRepository(DbConnectionFactory db) => _db = db;

    public async Task<string> CreateTokenAsync(int userId, TimeSpan expiry)
    {
        var token     = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                              .Replace("+", "-").Replace("/", "_").TrimEnd('=');
        var expiresAt = DateTime.UtcNow.Add(expiry);

        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            INSERT INTO PasswordResetTokens (UserId, Token, ExpiresAt)
            VALUES (@UserId, @Token, @ExpiresAt)", conn);
        cmd.Parameters.AddWithValue("@UserId",    userId);
        cmd.Parameters.AddWithValue("@Token",     token);
        cmd.Parameters.AddWithValue("@ExpiresAt", expiresAt);
        await cmd.ExecuteNonQueryAsync();
        return token;
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            SELECT Id, UserId, Token, ExpiresAt, UsedAt, CreatedAt
            FROM   PasswordResetTokens
            WHERE  Token = @Token", conn);
        cmd.Parameters.AddWithValue("@Token", token);
        using var r = await cmd.ExecuteReaderAsync();
        if (!await r.ReadAsync()) return null;
        return new PasswordResetToken
        {
            Id        = r.GetInt32(0),
            UserId    = r.GetInt32(1),
            Token     = r.GetString(2),
            ExpiresAt = r.GetDateTime(3),
            UsedAt    = r.IsDBNull(4) ? null : r.GetDateTime(4),
            CreatedAt = r.GetDateTime(5)
        };
    }

    public async Task MarkUsedAsync(int tokenId)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "UPDATE PasswordResetTokens SET UsedAt = GETUTCDATE() WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", tokenId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task InvalidatePreviousTokensAsync(int userId)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            UPDATE PasswordResetTokens
            SET    UsedAt = GETUTCDATE()
            WHERE  UserId = @UserId AND UsedAt IS NULL", conn);
        cmd.Parameters.AddWithValue("@UserId", userId);
        await cmd.ExecuteNonQueryAsync();
    }
}
