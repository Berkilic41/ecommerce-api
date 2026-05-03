using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository           _users;
    private readonly IPasswordResetRepository  _resetTokens;
    private readonly IEmailSender              _email;
    private readonly JwtService                _jwt;

    public AuthService(
        IUserRepository users,
        IPasswordResetRepository resetTokens,
        IEmailSender email,
        JwtService jwt)
    {
        _users       = users;
        _resetTokens = resetTokens;
        _email       = email;
        _jwt         = jwt;
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

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, string baseUrl)
    {
        var user = await _users.GetByEmailAsync(request.Email);

        // Always return 200 — don't reveal whether email exists (security best practice)
        if (user is null) return;

        await _resetTokens.InvalidatePreviousTokensAsync(user.Id);
        var token = await _resetTokens.CreateTokenAsync(user.Id, expiry: TimeSpan.FromHours(1));

        var resetLink = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(token)}";
        await _email.SendAsync(
            to:       user.Email,
            subject:  "Reset your password",
            htmlBody: $"""
                <p>Hi {user.Username},</p>
                <p>You requested a password reset. Click the link below to set a new password.
                   The link expires in <strong>1 hour</strong>.</p>
                <p><a href="{resetLink}">{resetLink}</a></p>
                <p>If you did not request this, you can safely ignore this email.</p>
                """);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var tokenRecord = await _resetTokens.GetByTokenAsync(request.Token)
            ?? throw new InvalidOperationException("Password reset token is invalid.");

        if (!tokenRecord.IsValid)
            throw new InvalidOperationException(
                tokenRecord.IsExpired ? "Reset token has expired." : "Reset token has already been used.");

        var user = await _users.GetByIdAsync(tokenRecord.UserId)
            ?? throw new InvalidOperationException("User not found.");

        var (hash, salt) = JwtService.HashPassword(request.NewPassword);
        await _users.UpdatePasswordAsync(user.Id, hash, salt);
        await _resetTokens.MarkUsedAsync(tokenRecord.Id);
    }

    private async Task<AuthResponse> IssueTokens(User user)
    {
        var access = _jwt.GenerateAccessToken(user);
        var refresh = JwtService.GenerateRefreshToken();
        await _users.SaveRefreshTokenAsync(user.Id, refresh, DateTime.UtcNow.AddDays(7));
        return new AuthResponse(access, refresh, user.Username, user.Email, user.Role);
    }
}
