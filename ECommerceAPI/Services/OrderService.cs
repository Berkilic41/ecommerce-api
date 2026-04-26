using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orders;
    private readonly ICartRepository _carts;
    private readonly IProductRepository _products;

    private static readonly HashSet<string> ValidStatuses =
        ["Pending", "Processing", "Shipped", "Delivered", "Cancelled"];

    public OrderService(IOrderRepository orders, ICartRepository carts, IProductRepository products)
    {
        _orders = orders;
        _carts = carts;
        _products = products;
    }

    public async Task<IEnumerable<OrderResponse>> GetUserOrdersAsync(int userId)
        => (await _orders.GetByUserIdAsync(userId)).Select(Map);

    public async Task<OrderResponse> GetByIdAsync(int id, int userId, bool isAdmin)
    {
        var order = await _orders.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Order {id} not found.");

        if (!isAdmin && order.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        return Map(order);
    }

    public async Task<OrderResponse> PlaceOrderAsync(int userId, PlaceOrderRequest request)
    {
        var cart = await _carts.GetByUserIdAsync(userId)
            ?? throw new InvalidOperationException("Cart is empty.");

        if (cart.Items.Count == 0)
            throw new InvalidOperationException("Cart is empty.");

        foreach (var item in cart.Items)
        {
            var product = await _products.GetByIdAsync(item.ProductId)
                ?? throw new InvalidOperationException($"Product {item.ProductId} is no longer available.");

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}");
        }

        var orderItems = cart.Items.Select(i => new OrderItem
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName!,
            Price = i.ProductPrice!.Value,
            Quantity = i.Quantity
        }).ToList();

        var order = new Order
        {
            UserId = userId,
            TotalAmount = orderItems.Sum(i => i.Price * i.Quantity),
            Status = "Pending",
            ShippingAddress = request.ShippingAddress
        };

        var orderId = await _orders.CreateAsync(order, orderItems);
        await _carts.ClearCartAsync(cart.Id);

        return Map((await _orders.GetByIdAsync(orderId))!);
    }

    public async Task<OrderResponse> UpdateStatusAsync(int orderId, UpdateOrderStatusRequest request)
    {
        if (!ValidStatuses.Contains(request.Status))
            throw new ArgumentException(
                $"Invalid status. Valid values: {string.Join(", ", ValidStatuses)}");

        var order = await _orders.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        await _orders.UpdateStatusAsync(orderId, request.Status);
        order.Status = request.Status;
        return Map(order);
    }

    private static OrderResponse Map(Order o) => new(
        o.Id, o.TotalAmount, o.Status, o.ShippingAddress, o.CreatedAt, o.UpdatedAt,
        o.Items.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.ProductName, i.Price, i.Quantity, i.Price * i.Quantity)));
}
