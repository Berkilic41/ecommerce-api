using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace ECommerceAPI.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _sut = new ProductService(_products.Object, _categories.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsKeyNotFound_WhenMissing()
    {
        _products.Setup(p => p.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        var act = () => _sut.GetByIdAsync(99);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMappedResponse()
    {
        _products.Setup(p => p.GetByIdAsync(1)).ReturnsAsync(new Product
        {
            Id = 1, Name = "Widget", Price = 9.99m, StockQuantity = 5,
            CategoryId = 2, CategoryName = "Tools", IsActive = true
        });

        var r = await _sut.GetByIdAsync(1);

        r.Id.Should().Be(1);
        r.Name.Should().Be("Widget");
        r.CategoryName.Should().Be("Tools");
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenCategoryMissing()
    {
        _categories.Setup(c => c.ExistsAsync(99)).ReturnsAsync(false);

        var req = new CreateProductRequest("X", null, 1m, 1, 99, null);
        var act = () => _sut.CreateAsync(req);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _products.Verify(p => p.CreateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_ClampsPageSize()
    {
        _products.Setup(p => p.GetAllAsync(null, null, 1, 100))
                 .ReturnsAsync((Array.Empty<Product>(), 0));

        await _sut.GetAllAsync(null, null, 0, 9999);

        _products.Verify(p => p.GetAllAsync(null, null, 1, 100), Times.Once);
    }
}
