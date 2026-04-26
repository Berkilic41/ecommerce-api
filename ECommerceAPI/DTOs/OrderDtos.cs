using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTOs;

public record PlaceOrderRequest(
    [Required, MaxLength(500)] string ShippingAddress
);

public record UpdateOrderStatusRequest(
    [Required] string Status
);

public record OrderItemResponse(
    int Id,
    int ProductId,
    string ProductName,
    decimal Price,
    int Quantity,
    decimal Subtotal
);

public record OrderResponse(
    int Id,
    decimal TotalAmount,
    string Status,
    string ShippingAddress,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IEnumerable<OrderItemResponse> Items
);
