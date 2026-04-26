using ECommerceAPI.Models;

namespace ECommerceAPI.Repositories.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(int userId);
    Task<int> GetOrCreateCartAsync(int userId);
    Task AddOrUpdateItemAsync(int cartId, int productId, int quantity);
    Task UpdateItemQuantityAsync(int cartId, int productId, int quantity);
    Task RemoveItemAsync(int cartId, int productId);
    Task ClearCartAsync(int cartId);
    Task<CartItem?> GetItemAsync(int cartId, int productId);
}
