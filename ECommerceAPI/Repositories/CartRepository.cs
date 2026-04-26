using ECommerceAPI.Data;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Repositories;

public class CartRepository : ICartRepository
{
    private readonly DbConnectionFactory _factory;

    public CartRepository(DbConnectionFactory factory) => _factory = factory;

    public async Task<Cart?> GetByUserIdAsync(int userId)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();

        using var cartCmd = new SqlCommand(
            "SELECT Id, UserId, CreatedAt FROM Carts WHERE UserId = @UserId", conn);
        cartCmd.Parameters.AddWithValue("@UserId", userId);
        using var cartReader = await cartCmd.ExecuteReaderAsync();
        if (!await cartReader.ReadAsync()) return null;

        var cart = new Cart
        {
            Id = cartReader.GetInt32(0),
            UserId = cartReader.GetInt32(1),
            CreatedAt = cartReader.GetDateTime(2)
        };
        await cartReader.CloseAsync();

        using var itemsCmd = new SqlCommand(@"
            SELECT ci.Id, ci.CartId, ci.ProductId, ci.Quantity,
                   p.Name, p.Price, p.ImageUrl, p.StockQuantity
            FROM CartItems ci
            INNER JOIN Products p ON ci.ProductId = p.Id
            WHERE ci.CartId = @CartId", conn);
        itemsCmd.Parameters.AddWithValue("@CartId", cart.Id);
        using var ir = await itemsCmd.ExecuteReaderAsync();
        while (await ir.ReadAsync())
        {
            cart.Items.Add(new CartItem
            {
                Id = ir.GetInt32(0),
                CartId = ir.GetInt32(1),
                ProductId = ir.GetInt32(2),
                Quantity = ir.GetInt32(3),
                ProductName = ir.GetString(4),
                ProductPrice = ir.GetDecimal(5),
                ProductImageUrl = ir.IsDBNull(6) ? null : ir.GetString(6),
                StockQuantity = ir.GetInt32(7)
            });
        }

        return cart;
    }

    public async Task<int> GetOrCreateCartAsync(int userId)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();

        using var sel = new SqlCommand("SELECT Id FROM Carts WHERE UserId = @UserId", conn);
        sel.Parameters.AddWithValue("@UserId", userId);
        var existing = await sel.ExecuteScalarAsync();
        if (existing is not null) return (int)existing;

        using var ins = new SqlCommand(
            "INSERT INTO Carts (UserId) OUTPUT INSERTED.Id VALUES (@UserId)", conn);
        ins.Parameters.AddWithValue("@UserId", userId);
        return (int)(await ins.ExecuteScalarAsync())!;
    }

    public async Task AddOrUpdateItemAsync(int cartId, int productId, int quantity)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            IF EXISTS (SELECT 1 FROM CartItems WHERE CartId = @CartId AND ProductId = @ProductId)
                UPDATE CartItems SET Quantity = Quantity + @Qty
                WHERE CartId = @CartId AND ProductId = @ProductId
            ELSE
                INSERT INTO CartItems (CartId, ProductId, Quantity)
                VALUES (@CartId, @ProductId, @Qty)", conn);
        cmd.Parameters.AddWithValue("@CartId", cartId);
        cmd.Parameters.AddWithValue("@ProductId", productId);
        cmd.Parameters.AddWithValue("@Qty", quantity);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateItemQuantityAsync(int cartId, int productId, int quantity)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "UPDATE CartItems SET Quantity = @Qty WHERE CartId = @CartId AND ProductId = @ProductId", conn);
        cmd.Parameters.AddWithValue("@CartId", cartId);
        cmd.Parameters.AddWithValue("@ProductId", productId);
        cmd.Parameters.AddWithValue("@Qty", quantity);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task RemoveItemAsync(int cartId, int productId)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "DELETE FROM CartItems WHERE CartId = @CartId AND ProductId = @ProductId", conn);
        cmd.Parameters.AddWithValue("@CartId", cartId);
        cmd.Parameters.AddWithValue("@ProductId", productId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task ClearCartAsync(int cartId)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("DELETE FROM CartItems WHERE CartId = @CartId", conn);
        cmd.Parameters.AddWithValue("@CartId", cartId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<CartItem?> GetItemAsync(int cartId, int productId)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "SELECT Id, CartId, ProductId, Quantity FROM CartItems WHERE CartId = @CartId AND ProductId = @ProductId",
            conn);
        cmd.Parameters.AddWithValue("@CartId", cartId);
        cmd.Parameters.AddWithValue("@ProductId", productId);
        using var r = await cmd.ExecuteReaderAsync();
        if (!await r.ReadAsync()) return null;
        return new CartItem { Id = r.GetInt32(0), CartId = r.GetInt32(1), ProductId = r.GetInt32(2), Quantity = r.GetInt32(3) };
    }
}
