using System.ComponentModel.DataAnnotations;

// ─── Password Reset DTOs ─────────────────────────────────────────────────────

namespace ECommerceAPI.DTOs;

public record RegisterRequest(
    [Required, MinLength(3), MaxLength(50)] string Username,
    [Required, EmailAddress, MaxLength(100)] string Email,
    [Required, MinLength(6)] string Password
);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record RefreshTokenRequest(
    [Required] string RefreshToken
);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string Username,
    string Email,
    string Role
);

public record ForgotPasswordRequest(
    [Required, EmailAddress] string Email
);

public record ResetPasswordRequest(
    [Required] string Token,
    [Required, MinLength(8)] string NewPassword,
    [Required] string ConfirmPassword
);
