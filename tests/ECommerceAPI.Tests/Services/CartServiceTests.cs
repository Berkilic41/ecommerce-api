using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace ECommerceAPI.Tests.Services;

public class CartServiceTests
{
    private readonly Mock<ICartRepository> _carts = new();
    private readonly Mock<IProductRepository> _products = new();
    private readonly CartService _sut;

    public CartServiceTests()
    {
        _sut = new CartService(_carts.Object, _products.Object);
    }

    [Fact]
    public async Task AddItemAsync_ThrowsWhenProductMissing()
    {
        _products.Setup(p => p.GetByIdAsync(7)).ReturnsAsync((Product?)null);

        var act = () => _sut.AddItemAsync(1, new AddToCartRequest(7, 1));

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task AddItemAsync_ThrowsWhenInsufficientStock()
    {
        _products.Setup(p => p.GetByIdAsync(7)).ReturnsAsync(new Product { Id = 7, Name = "x", StockQuantity = 2 });

        var act = () => _sut.AddItemAsync(1, new AddToCartRequest(7, 5));

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*Available: 2*");
    }

    [Fact]
    public async Task AddItemAsync_HappyPath_DelegatesToRepo()
    {
        _products.Setup(p => p.GetByIdAsync(7))
                 .ReturnsAsync(new Product { Id = 7, Name = "x", Price = 5m, StockQuantity = 10 });
        _carts.Setup(c => c.GetOrCreateCartAsync(1)).ReturnsAsync(50);
        _carts.Setup(c => c.GetByUserIdAsync(1)).ReturnsAsync(new Cart
        {
            Id = 50, UserId = 1,
            Items = [new CartItem { ProductId = 7, Quantity = 3, ProductName = "x", ProductPrice = 5m }]
        });

        var result = await _sut.AddItemAsync(1, new AddToCartRequest(7, 3));

        _carts.Verify(c => c.AddOrUpdateItemAsync(50, 7, 3), Times.Once);
        result.Total.Should().Be(15m);
    }

    [Fact]
    public async Task GetCartAsync_ReturnsEmptyWhenNoCart()
    {
        _carts.Setup(c => c.GetByUserIdAsync(1)).ReturnsAsync((Cart?)null);

        var result = await _sut.GetCartAsync(1);

        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }
}
