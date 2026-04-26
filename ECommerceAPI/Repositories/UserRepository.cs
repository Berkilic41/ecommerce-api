using ECommerceAPI.Data;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _factory;

    public UserRepository(DbConnectionFactory factory) => _factory = factory;

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "SELECT Id, Username, Email, PasswordHash, PasswordSalt, Role, CreatedAt FROM Users WHERE Email = @Email",
            conn);
        cmd.Parameters.AddWithValue("@Email", email);
        using var r = await cmd.ExecuteReaderAsync();
        return await r.ReadAsync() ? MapUser(r) : null;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "SELECT Id, Username, Email, PasswordHash, PasswordSalt, Role, CreatedAt FROM Users WHERE Id = @Id",
            conn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var r = await cmd.ExecuteReaderAsync();
        return await r.ReadAsync() ? MapUser(r) : null;
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("SELECT COUNT(1) FROM Users WHERE Email = @Email", conn);
        cmd.Parameters.AddWithValue("@Email", email);
        return (int)(await cmd.ExecuteScalarAsync())! > 0;
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("SELECT COUNT(1) FROM Users WHERE Username = @Username", conn);
        cmd.Parameters.AddWithValue("@Username", username);
        return (int)(await cmd.ExecuteScalarAsync())! > 0;
    }

    public async Task<int> CreateAsync(User user)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            @"INSERT INTO Users (Username, Email, PasswordHash, PasswordSalt, Role)
              OUTPUT INSERTED.Id
              VALUES (@Username, @Email, @PasswordHash, @PasswordSalt, @Role)",
            conn);
        cmd.Parameters.AddWithValue("@Username", user.Username);
        cmd.Parameters.AddWithValue("@Email", user.Email);
        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@PasswordSalt", user.PasswordSalt);
        cmd.Parameters.AddWithValue("@Role", user.Role);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task SaveRefreshTokenAsync(int userId, string token, DateTime expiresAt)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "INSERT INTO RefreshTokens (UserId, Token, ExpiresAt) VALUES (@UserId, @Token, @ExpiresAt)",
            conn);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@Token", token);
        cmd.Parameters.AddWithValue("@ExpiresAt", expiresAt);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<(int UserId, bool IsRevoked, DateTime ExpiresAt)?> GetRefreshTokenAsync(string token)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "SELECT UserId, IsRevoked, ExpiresAt FROM RefreshTokens WHERE Token = @Token",
            conn);
        cmd.Parameters.AddWithValue("@Token", token);
        using var r = await cmd.ExecuteReaderAsync();
        if (!await r.ReadAsync()) return null;
        return (r.GetInt32(0), r.GetBoolean(1), r.GetDateTime(2));
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "UPDATE RefreshTokens SET IsRevoked = 1 WHERE Token = @Token", conn);
        cmd.Parameters.AddWithValue("@Token", token);
        await cmd.ExecuteNonQueryAsync();
    }

    private static User MapUser(SqlDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Username = r.GetString(1),
        Email = r.GetString(2),
        PasswordHash = r.GetString(3),
        PasswordSalt = r.GetString(4),
        Role = r.GetString(5),
        CreatedAt = r.GetDateTime(6)
    };
}
