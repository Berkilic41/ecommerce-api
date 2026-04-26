using ECommerceAPI.Models;

namespace ECommerceAPI.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByUsernameAsync(string username);
    Task<int> CreateAsync(User user);
    Task SaveRefreshTokenAsync(int userId, string token, DateTime expiresAt);
    Task<(int UserId, bool IsRevoked, DateTime ExpiresAt)?> GetRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
}
