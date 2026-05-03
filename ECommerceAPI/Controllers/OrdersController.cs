using Asp.Versioning;
using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders) => _orders = orders;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin => User.IsInRole("Admin");

    /// <summary>Get the current user's order history.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetMyOrders()
        => Ok(await _orders.GetUserOrdersAsync(UserId));

    /// <summary>Get a specific order. Users can only see their own orders.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(int id)
        => Ok(await _orders.GetByIdAsync(id, UserId, IsAdmin));

    /// <summary>Place an order from the current cart contents.</summary>
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        var result = await _orders.PlaceOrderAsync(UserId, request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update order status. Admin only.</summary>
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrderResponse>> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        => Ok(await _orders.UpdateStatusAsync(id, request));
}
