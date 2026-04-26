using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTOs;

public record AddToCartRequest(
    [Required] int ProductId,
    [Required, Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")] int Quantity
);

public record UpdateCartItemRequest(
    [Required, Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")] int Quantity
);

public record CartItemResponse(
    int Id,
    int ProductId,
    string ProductName,
    decimal ProductPrice,
    string? ProductImageUrl,
    int Quantity,
    decimal Subtotal
);

public record CartResponse(
    int Id,
    IEnumerable<CartItemResponse> Items,
    decimal Total
);
