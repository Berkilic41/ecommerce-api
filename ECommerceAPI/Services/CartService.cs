using ECommerceAPI.DTOs;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _carts;
    private readonly IProductRepository _products;

    public CartService(ICartRepository carts, IProductRepository products)
    {
        _carts = carts;
        _products = products;
    }

    public async Task<CartResponse> GetCartAsync(int userId)
    {
        var cart = await _carts.GetByUserIdAsync(userId);
        if (cart is null) return new CartResponse(0, [], 0);
        var items = cart.Items.Select(ToItemResponse).ToList();
        return new CartResponse(cart.Id, items, items.Sum(i => i.Subtotal));
    }

    public async Task<CartResponse> AddItemAsync(int userId, AddToCartRequest request)
    {
        var product = await _products.GetByIdAsync(request.ProductId)
            ?? throw new KeyNotFoundException($"Product {request.ProductId} not found.");

        if (product.StockQuantity < request.Quantity)
            throw new InvalidOperationException($"Insufficient stock. Available: {product.StockQuantity}");

        var cartId = await _carts.GetOrCreateCartAsync(userId);
        await _carts.AddOrUpdateItemAsync(cartId, request.ProductId, request.Quantity);
        return await GetCartAsync(userId);
    }

    public async Task<CartResponse> UpdateItemAsync(int userId, int productId, UpdateCartItemRequest request)
    {
        var cart = await _carts.GetByUserIdAsync(userId)
            ?? throw new InvalidOperationException("Cart not found.");

        if (await _carts.GetItemAsync(cart.Id, productId) is null)
            throw new KeyNotFoundException("Item not found in cart.");

        var product = await _products.GetByIdAsync(productId)
            ?? throw new KeyNotFoundException($"Product {productId} not found.");

        if (product.StockQuantity < request.Quantity)
            throw new InvalidOperationException($"Insufficient stock. Available: {product.StockQuantity}");

        await _carts.UpdateItemQuantityAsync(cart.Id, productId, request.Quantity);
        return await GetCartAsync(userId);
    }

    public async Task<CartResponse> RemoveItemAsync(int userId, int productId)
    {
        var cart = await _carts.GetByUserIdAsync(userId)
            ?? throw new InvalidOperationException("Cart not found.");
        await _carts.RemoveItemAsync(cart.Id, productId);
        return await GetCartAsync(userId);
    }

    public async Task ClearCartAsync(int userId)
    {
        var cart = await _carts.GetByUserIdAsync(userId);
        if (cart is not null) await _carts.ClearCartAsync(cart.Id);
    }

    private static CartItemResponse ToItemResponse(Models.CartItem i) => new(
        i.Id, i.ProductId, i.ProductName!, i.ProductPrice!.Value,
        i.ProductImageUrl, i.Quantity, i.ProductPrice.Value * i.Quantity);
}
