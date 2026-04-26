using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTOs;

public record ProductResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    int CategoryId,
    string? CategoryName,
    string? ImageUrl,
    bool IsActive,
    DateTime CreatedAt
);

public record ProductListResponse(
    IEnumerable<ProductResponse> Products,
    int TotalCount,
    int Page,
    int PageSize
);

public record CreateProductRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(2000)] string? Description,
    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")] decimal Price,
    [Required, Range(0, int.MaxValue)] int StockQuantity,
    [Required] int CategoryId,
    [MaxLength(500)] string? ImageUrl
);

public record UpdateProductRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(2000)] string? Description,
    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")] decimal Price,
    [Required, Range(0, int.MaxValue)] int StockQuantity,
    [Required] int CategoryId,
    [MaxLength(500)] string? ImageUrl,
    bool IsActive = true
);
