using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo) => _repo = repo;

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
        => (await _repo.GetAllAsync()).Select(Map);

    public async Task<CategoryResponse> GetByIdAsync(int id)
        => Map(await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Category {id} not found."));

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var id = await _repo.CreateAsync(new Category { Name = request.Name, Description = request.Description });
        return await GetByIdAsync(id);
    }

    public async Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var cat = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Category {id} not found.");
        cat.Name = request.Name;
        cat.Description = request.Description;
        await _repo.UpdateAsync(cat);
        return Map(cat);
    }

    public async Task DeleteAsync(int id)
    {
        if (!await _repo.ExistsAsync(id))
            throw new KeyNotFoundException($"Category {id} not found.");
        await _repo.DeleteAsync(id);
    }

    private static CategoryResponse Map(Category c) => new(c.Id, c.Name, c.Description);
}
