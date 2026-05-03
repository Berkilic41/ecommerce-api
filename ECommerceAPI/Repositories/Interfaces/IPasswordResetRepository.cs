using ECommerceAPI.Models;

namespace ECommerceAPI.Repositories.Interfaces;

public interface IPasswordResetRepository
{
    Task<string> CreateTokenAsync(int userId, TimeSpan expiry);
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task MarkUsedAsync(int tokenId);
    Task InvalidatePreviousTokensAsync(int userId);
}
