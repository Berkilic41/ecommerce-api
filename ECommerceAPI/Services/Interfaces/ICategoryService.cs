using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse> GetByIdAsync(int id);
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
    Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryRequest request);
    Task DeleteAsync(int id);
}
