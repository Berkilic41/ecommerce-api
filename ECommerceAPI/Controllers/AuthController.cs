using Asp.Versioning;
using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerceAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Register a new user account.</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var result = await _auth.RegisterAsync(request);
        return StatusCode(201, result);
    }

    /// <summary>Login and receive access + refresh tokens.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        => Ok(await _auth.LoginAsync(request));

    /// <summary>Exchange a valid refresh token for a new token pair.</summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request)
        => Ok(await _auth.RefreshTokenAsync(request));

    /// <summary>Revoke the provided refresh token.</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await _auth.LogoutAsync(request.RefreshToken);
        return NoContent();
    }

    /// <summary>
    /// Request a password reset email. Always returns 200 regardless of whether
    /// the email exists (prevents user enumeration).
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        await _auth.ForgotPasswordAsync(request, baseUrl);
        return Ok(new { message = "If an account with that email exists, a reset link has been sent." });
    }

    /// <summary>Reset password using a valid token from the forgot-password email.</summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _auth.ResetPasswordAsync(request);
        return Ok(new { message = "Password has been reset successfully. You can now log in." });
    }
}
