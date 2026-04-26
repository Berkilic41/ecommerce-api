using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderResponse>> GetUserOrdersAsync(int userId);
    Task<OrderResponse> GetByIdAsync(int id, int userId, bool isAdmin);
    Task<OrderResponse> PlaceOrderAsync(int userId, PlaceOrderRequest request);
    Task<OrderResponse> UpdateStatusAsync(int orderId, UpdateOrderStatusRequest request);
}
