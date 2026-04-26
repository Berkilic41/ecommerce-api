using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTOs;

public record CategoryResponse(int Id, string Name, string? Description);

public record CreateCategoryRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description
);

public record UpdateCategoryRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description
);
