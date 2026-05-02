using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerceAPI.Services.Decorators;

/// <summary>
/// Decorator over <see cref="IProductService"/> that caches individual product lookups.
/// GetByIdAsync is called on every cart operation — caching it eliminates N stock-check round-trips.
/// Writes invalidate the relevant cache entries.
/// </summary>
public class CachedProductService : IProductService
{
    private static string ByIdKey(int id) => $"products:id:{id}";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly IProductService _inner;
    private readonly IMemoryCache    _cache;

    public CachedProductService(IProductService inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public Task<ProductListResponse> GetAllAsync(string? search, int? categoryId, int page, int pageSize)
        => _inner.GetAllAsync(search, categoryId, page, pageSize);

    public async Task<ProductResponse> GetByIdAsync(int id)
    {
        if (_cache.TryGetValue(ByIdKey(id), out ProductResponse? cached) && cached is not null)
            return cached;

        var fresh = await _inner.GetByIdAsync(id);
        _cache.Set(ByIdKey(id), fresh, CacheTtl);
        return fresh;
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        var result = await _inner.CreateAsync(request);
        return result;
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request)
    {
        var result = await _inner.UpdateAsync(id, request);
        _cache.Remove(ByIdKey(id));
        return result;
    }

    public async Task DeleteAsync(int id)
    {
        await _inner.DeleteAsync(id);
        _cache.Remove(ByIdKey(id));
    }
}
