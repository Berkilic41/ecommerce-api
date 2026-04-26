using ECommerceAPI.Data;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly DbConnectionFactory _factory;

    public OrderRepository(DbConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();

        using var cmd = new SqlCommand(@"
            SELECT Id, UserId, TotalAmount, Status, ShippingAddress, CreatedAt, UpdatedAt
            FROM Orders WHERE UserId = @UserId ORDER BY CreatedAt DESC", conn);
        cmd.Parameters.AddWithValue("@UserId", userId);

        var orders = new List<Order>();
        using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
            orders.Add(MapOrder(r));
        await r.CloseAsync();

        foreach (var order in orders)
        {
            using var ic = new SqlCommand(
                "SELECT Id, OrderId, ProductId, ProductName, Price, Quantity FROM OrderItems WHERE OrderId = @Id",
                conn);
            ic.Parameters.AddWithValue("@Id", order.Id);
            using var ir = await ic.ExecuteReaderAsync();
            while (await ir.ReadAsync())
                order.Items.Add(MapItem(ir));
            await ir.CloseAsync();
        }

        return orders;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();

        using var cmd = new SqlCommand(@"
            SELECT Id, UserId, TotalAmount, Status, ShippingAddress, CreatedAt, UpdatedAt
            FROM Orders WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var r = await cmd.ExecuteReaderAsync();
        if (!await r.ReadAsync()) return null;
        var order = MapOrder(r);
        await r.CloseAsync();

        using var ic = new SqlCommand(
            "SELECT Id, OrderId, ProductId, ProductName, Price, Quantity FROM OrderItems WHERE OrderId = @Id",
            conn);
        ic.Parameters.AddWithValue("@Id", order.Id);
        using var ir = await ic.ExecuteReaderAsync();
        while (await ir.ReadAsync())
            order.Items.Add(MapItem(ir));

        return order;
    }

    public async Task<int> CreateAsync(Order order, IEnumerable<OrderItem> items)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var tx = conn.BeginTransaction();
        try
        {
            using var orderCmd = new SqlCommand(@"
                INSERT INTO Orders (UserId, TotalAmount, Status, ShippingAddress)
                OUTPUT INSERTED.Id
                VALUES (@UserId, @Total, @Status, @Address)", conn, tx);
            orderCmd.Parameters.AddWithValue("@UserId", order.UserId);
            orderCmd.Parameters.AddWithValue("@Total", order.TotalAmount);
            orderCmd.Parameters.AddWithValue("@Status", order.Status);
            orderCmd.Parameters.AddWithValue("@Address", order.ShippingAddress);
            var orderId = (int)(await orderCmd.ExecuteScalarAsync())!;

            foreach (var item in items)
            {
                using var itemCmd = new SqlCommand(@"
                    INSERT INTO OrderItems (OrderId, ProductId, ProductName, Price, Quantity)
                    VALUES (@OrderId, @ProductId, @Name, @Price, @Qty)", conn, tx);
                itemCmd.Parameters.AddWithValue("@OrderId", orderId);
                itemCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                itemCmd.Parameters.AddWithValue("@Name", item.ProductName);
                itemCmd.Parameters.AddWithValue("@Price", item.Price);
                itemCmd.Parameters.AddWithValue("@Qty", item.Quantity);
                await itemCmd.ExecuteNonQueryAsync();

                using var stockCmd = new SqlCommand(
                    "UPDATE Products SET StockQuantity = StockQuantity - @Qty WHERE Id = @Id",
                    conn, tx);
                stockCmd.Parameters.AddWithValue("@Qty", item.Quantity);
                stockCmd.Parameters.AddWithValue("@Id", item.ProductId);
                await stockCmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return orderId;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateStatusAsync(int orderId, string status)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "UPDATE Orders SET Status = @Status, UpdatedAt = GETUTCDATE() WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Status", status);
        cmd.Parameters.AddWithValue("@Id", orderId);
        await cmd.ExecuteNonQueryAsync();
    }

    private static Order MapOrder(SqlDataReader r) => new()
    {
        Id = r.GetInt32(0), UserId = r.GetInt32(1), TotalAmount = r.GetDecimal(2),
        Status = r.GetString(3), ShippingAddress = r.GetString(4),
        CreatedAt = r.GetDateTime(5), UpdatedAt = r.GetDateTime(6)
    };

    private static OrderItem MapItem(SqlDataReader r) => new()
    {
        Id = r.GetInt32(0), OrderId = r.GetInt32(1), ProductId = r.GetInt32(2),
        ProductName = r.GetString(3), Price = r.GetDecimal(4), Quantity = r.GetInt32(5)
    };
}
