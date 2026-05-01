using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerceAPI.Services.Decorators;

/// <summary>
/// Decorator over <see cref="ICategoryService"/> that caches read paths in memory.
/// Categories are read-heavy and rarely changed, so this is a high-value cache.
/// Writes invalidate the cache.
/// </summary>
public class CachedCategoryService : ICategoryService
{
    private const string AllKey = "categories:all";
    private static string ByIdKey(int id) => $"categories:id:{id}";

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly ICategoryService _inner;
    private readonly IMemoryCache _cache;

    public CachedCategoryService(ICategoryService inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
    {
        if (_cache.TryGetValue(AllKey, out IEnumerable<CategoryResponse>? cached) && cached is not null)
            return cached;

        var fresh = await _inner.GetAllAsync();
        var materialized = fresh.ToList();
        _cache.Set(AllKey, (IEnumerable<CategoryResponse>)materialized, CacheTtl);
        return materialized;
    }

    public async Task<CategoryResponse> GetByIdAsync(int id)
    {
        if (_cache.TryGetValue(ByIdKey(id), out CategoryResponse? cached) && cached is not null)
            return cached;

        var fresh = await _inner.GetByIdAsync(id);
        _cache.Set(ByIdKey(id), fresh, CacheTtl);
        return fresh;
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var result = await _inner.CreateAsync(request);
        Invalidate();
        return result;
    }

    public async Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var result = await _inner.UpdateAsync(id, request);
        Invalidate(id);
        return result;
    }

    public async Task DeleteAsync(int id)
    {
        await _inner.DeleteAsync(id);
        Invalidate(id);
    }

    private void Invalidate(int? id = null)
    {
        _cache.Remove(AllKey);
        if (id.HasValue) _cache.Remove(ByIdKey(id.Value));
    }
}
