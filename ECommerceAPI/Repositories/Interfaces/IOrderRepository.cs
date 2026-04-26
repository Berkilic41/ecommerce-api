using ECommerceAPI.Models;

namespace ECommerceAPI.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
    Task<Order?> GetByIdAsync(int id);
    Task<int> CreateAsync(Order order, IEnumerable<OrderItem> items);
    Task UpdateStatusAsync(int orderId, string status);
}
