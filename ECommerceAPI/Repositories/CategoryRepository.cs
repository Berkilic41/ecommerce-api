using ECommerceAPI.Data;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly DbConnectionFactory _factory;

    public CategoryRepository(DbConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("SELECT Id, Name, Description FROM Categories ORDER BY Name", conn);
        using var r = await cmd.ExecuteReaderAsync();
        var list = new List<Category>();
        while (await r.ReadAsync())
            list.Add(Map(r));
        return list;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("SELECT Id, Name, Description FROM Categories WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var r = await cmd.ExecuteReaderAsync();
        return await r.ReadAsync() ? Map(r) : null;
    }

    public async Task<int> CreateAsync(Category category)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "INSERT INTO Categories (Name, Description) OUTPUT INSERTED.Id VALUES (@Name, @Desc)", conn);
        cmd.Parameters.AddWithValue("@Name", category.Name);
        cmd.Parameters.AddWithValue("@Desc", (object?)category.Description ?? DBNull.Value);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task UpdateAsync(Category category)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "UPDATE Categories SET Name = @Name, Description = @Desc WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", category.Id);
        cmd.Parameters.AddWithValue("@Name", category.Name);
        cmd.Parameters.AddWithValue("@Desc", (object?)category.Description ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("DELETE FROM Categories WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("SELECT COUNT(1) FROM Categories WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);
        return (int)(await cmd.ExecuteScalarAsync())! > 0;
    }

    private static Category Map(SqlDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Name = r.GetString(1),
        Description = r.IsDBNull(2) ? null : r.GetString(2)
    };
}
