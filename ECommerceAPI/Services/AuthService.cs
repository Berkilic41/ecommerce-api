using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly JwtService _jwt;

    public AuthService(IUserRepository users, JwtService jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _users.ExistsByEmailAsync(request.Email))
            throw new InvalidOperationException("Email is already in use.");

        if (await _users.ExistsByUsernameAsync(request.Username))
            throw new InvalidOperationException("Username is already taken.");

        var (hash, salt) = JwtService.HashPassword(request.Password);
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = "User"
        };

        user.Id = await _users.CreateAsync(user);
        return await IssueTokens(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _users.GetByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!JwtService.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return await IssueTokens(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var data = await _users.GetRefreshTokenAsync(request.RefreshToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (data.IsRevoked || data.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token is expired or revoked.");

        var user = await _users.GetByIdAsync(data.UserId)
            ?? throw new UnauthorizedAccessException("User not found.");

        await _users.RevokeRefreshTokenAsync(request.RefreshToken);
        return await IssueTokens(user);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _users.RevokeRefreshTokenAsync(refreshToken);
    }

    private async Task<AuthResponse> IssueTokens(User user)
    {
        var access = _jwt.GenerateAccessToken(user);
        var refresh = JwtService.GenerateRefreshToken();
        await _users.SaveRefreshTokenAsync(user.Id, refresh, DateTime.UtcNow.AddDays(7));
        return new AuthResponse(access, refresh, user.Username, user.Email, user.Role);
    }
}
