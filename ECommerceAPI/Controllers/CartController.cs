using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cart;

    public CartController(ICartService cart) => _cart = cart;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Get the current user's cart.</summary>
    [HttpGet]
    public async Task<ActionResult<CartResponse>> GetCart()
        => Ok(await _cart.GetCartAsync(UserId));

    /// <summary>Add a product to cart (increments quantity if already present).</summary>
    [HttpPost("items")]
    public async Task<ActionResult<CartResponse>> AddItem([FromBody] AddToCartRequest request)
        => Ok(await _cart.AddItemAsync(UserId, request));

    /// <summary>Set exact quantity for a cart item.</summary>
    [HttpPut("items/{productId:int}")]
    public async Task<ActionResult<CartResponse>> UpdateItem(int productId, [FromBody] UpdateCartItemRequest request)
        => Ok(await _cart.UpdateItemAsync(UserId, productId, request));

    /// <summary>Remove a single item from cart.</summary>
    [HttpDelete("items/{productId:int}")]
    public async Task<ActionResult<CartResponse>> RemoveItem(int productId)
        => Ok(await _cart.RemoveItemAsync(UserId, productId));

    /// <summary>Clear all items from cart.</summary>
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        await _cart.ClearCartAsync(UserId);
        return NoContent();
    }
}
