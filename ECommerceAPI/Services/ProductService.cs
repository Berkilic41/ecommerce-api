using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _products;
    private readonly ICategoryRepository _categories;

    public ProductService(IProductRepository products, ICategoryRepository categories)
    {
        _products = products;
        _categories = categories;
    }

    public async Task<ProductListResponse> GetAllAsync(string? search, int? categoryId, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var (items, total) = await _products.GetAllAsync(search, categoryId, page, pageSize);
        return new ProductListResponse(items.Select(Map), total, page, pageSize);
    }

    public async Task<ProductResponse> GetByIdAsync(int id)
        => Map(await _products.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Product {id} not found."));

    public async Task<ProductResponse> CreateAsync(CreateProductRequest req)
    {
        if (!await _categories.ExistsAsync(req.CategoryId))
            throw new KeyNotFoundException($"Category {req.CategoryId} not found.");

        var id = await _products.CreateAsync(new Product
        {
            Name = req.Name, Description = req.Description,
            Price = req.Price, StockQuantity = req.StockQuantity,
            CategoryId = req.CategoryId, ImageUrl = req.ImageUrl
        });
        return await GetByIdAsync(id);
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest req)
    {
        var product = await _products.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        if (!await _categories.ExistsAsync(req.CategoryId))
            throw new KeyNotFoundException($"Category {req.CategoryId} not found.");

        product.Name = req.Name; product.Description = req.Description;
        product.Price = req.Price; product.StockQuantity = req.StockQuantity;
        product.CategoryId = req.CategoryId; product.ImageUrl = req.ImageUrl;
        product.IsActive = req.IsActive;
        await _products.UpdateAsync(product);
        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        if (!await _products.ExistsAsync(id))
            throw new KeyNotFoundException($"Product {id} not found.");
        await _products.DeleteAsync(id);
    }

    private static ProductResponse Map(Product p)
        => new(p.Id, p.Name, p.Description, p.Price, p.StockQuantity,
               p.CategoryId, p.CategoryName, p.ImageUrl, p.IsActive, p.CreatedAt);
}
