using ECommerceAPI.Models;

namespace ECommerceAPI.Repositories.Interfaces;

public interface IProductRepository
{
    Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(string? search, int? categoryId, int page, int pageSize);
    Task<Product?> GetByIdAsync(int id);
    Task<int> CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
