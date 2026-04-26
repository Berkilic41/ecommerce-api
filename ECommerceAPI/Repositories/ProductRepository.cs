using ECommerceAPI.Data;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly DbConnectionFactory _factory;

    public ProductRepository(DbConnectionFactory factory) => _factory = factory;

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(
        string? search, int? categoryId, int page, int pageSize)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();

        var conditions = new List<string> { "p.IsActive = 1" };
        if (!string.IsNullOrWhiteSpace(search))
            conditions.Add("(p.Name LIKE @Search OR p.Description LIKE @Search)");
        if (categoryId.HasValue)
            conditions.Add("p.CategoryId = @CategoryId");

        var where = "WHERE " + string.Join(" AND ", conditions);

        using var countCmd = new SqlCommand($"SELECT COUNT(*) FROM Products p {where}", conn);
        using var dataCmd = new SqlCommand($@"
            SELECT p.Id, p.Name, p.Description, p.Price, p.StockQuantity,
                   p.CategoryId, c.Name, p.ImageUrl, p.IsActive, p.CreatedAt
            FROM Products p
            INNER JOIN Categories c ON p.CategoryId = c.Id
            {where}
            ORDER BY p.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", conn);

        if (!string.IsNullOrWhiteSpace(search))
        {
            countCmd.Parameters.AddWithValue("@Search", $"%{search}%");
            dataCmd.Parameters.AddWithValue("@Search", $"%{search}%");
        }
        if (categoryId.HasValue)
        {
            countCmd.Parameters.AddWithValue("@CategoryId", categoryId.Value);
            dataCmd.Parameters.AddWithValue("@CategoryId", categoryId.Value);
        }

        dataCmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
        dataCmd.Parameters.AddWithValue("@PageSize", pageSize);

        var total = (int)(await countCmd.ExecuteScalarAsync())!;

        using var r = await dataCmd.ExecuteReaderAsync();
        var products = new List<Product>();
        while (await r.ReadAsync())
            products.Add(Map(r));

        return (products, total);
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            SELECT p.Id, p.Name, p.Description, p.Price, p.StockQuantity,
                   p.CategoryId, c.Name, p.ImageUrl, p.IsActive, p.CreatedAt
            FROM Products p
            INNER JOIN Categories c ON p.CategoryId = c.Id
            WHERE p.Id = @Id AND p.IsActive = 1", conn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var r = await cmd.ExecuteReaderAsync();
        return await r.ReadAsync() ? Map(r) : null;
    }

    public async Task<int> CreateAsync(Product product)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            INSERT INTO Products (Name, Description, Price, StockQuantity, CategoryId, ImageUrl)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Desc, @Price, @Stock, @CatId, @Image)", conn);
        cmd.Parameters.AddWithValue("@Name", product.Name);
        cmd.Parameters.AddWithValue("@Desc", (object?)product.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Price", product.Price);
        cmd.Parameters.AddWithValue("@Stock", product.StockQuantity);
        cmd.Parameters.AddWithValue("@CatId", product.CategoryId);
        cmd.Parameters.AddWithValue("@Image", (object?)product.ImageUrl ?? DBNull.Value);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task UpdateAsync(Product product)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            UPDATE Products
            SET Name = @Name, Description = @Desc, Price = @Price,
                StockQuantity = @Stock, CategoryId = @CatId,
                ImageUrl = @Image, IsActive = @Active
            WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", product.Id);
        cmd.Parameters.AddWithValue("@Name", product.Name);
        cmd.Parameters.AddWithValue("@Desc", (object?)product.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Price", product.Price);
        cmd.Parameters.AddWithValue("@Stock", product.StockQuantity);
        cmd.Parameters.AddWithValue("@CatId", product.CategoryId);
        cmd.Parameters.AddWithValue("@Image", (object?)product.ImageUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Active", product.IsActive);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("UPDATE Products SET IsActive = 0 WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("SELECT COUNT(1) FROM Products WHERE Id = @Id AND IsActive = 1", conn);
        cmd.Parameters.AddWithValue("@Id", id);
        return (int)(await cmd.ExecuteScalarAsync())! > 0;
    }

    private static Product Map(SqlDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Name = r.GetString(1),
        Description = r.IsDBNull(2) ? null : r.GetString(2),
        Price = r.GetDecimal(3),
        StockQuantity = r.GetInt32(4),
        CategoryId = r.GetInt32(5),
        CategoryName = r.GetString(6),
        ImageUrl = r.IsDBNull(7) ? null : r.GetString(7),
        IsActive = r.GetBoolean(8),
        CreatedAt = r.GetDateTime(9)
    };
}
