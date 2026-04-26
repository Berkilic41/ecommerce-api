using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface IProductService
{
    Task<ProductListResponse> GetAllAsync(string? search, int? categoryId, int page, int pageSize);
    Task<ProductResponse> GetByIdAsync(int id);
    Task<ProductResponse> CreateAsync(CreateProductRequest request);
    Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request);
    Task DeleteAsync(int id);
}
