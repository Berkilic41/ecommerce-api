using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace ECommerceAPI.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly JwtService _jwt;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"]           = "test-key-must-be-at-least-32-characters-long-12345",
            ["Jwt:Issuer"]        = "test",
            ["Jwt:Audience"]      = "test",
            ["Jwt:ExpiryMinutes"] = "15"
        }).Build();
        _jwt = new JwtService(config);
        _sut = new AuthService(_users.Object, _jwt);
    }

    [Fact]
    public async Task RegisterAsync_RejectsDuplicateEmail()
    {
        _users.Setup(r => r.ExistsByEmailAsync("dup@x.com")).ReturnsAsync(true);

        var act = () => _sut.RegisterAsync(new RegisterRequest("user", "dup@x.com", "password123"));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already*");
    }

    [Fact]
    public async Task RegisterAsync_RejectsDuplicateUsername()
    {
        _users.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
        _users.Setup(r => r.ExistsByUsernameAsync("taken")).ReturnsAsync(true);

        var act = () => _sut.RegisterAsync(new RegisterRequest("taken", "ok@x.com", "password123"));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already*");
    }

    [Fact]
    public async Task RegisterAsync_HappyPath_CreatesUserAndReturnsTokens()
    {
        _users.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
        _users.Setup(r => r.ExistsByUsernameAsync(It.IsAny<string>())).ReturnsAsync(false);
        _users.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(42);

        var result = await _sut.RegisterAsync(new RegisterRequest("alice", "alice@x.com", "password123"));

        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.Username.Should().Be("alice");
        result.Email.Should().Be("alice@x.com");
        result.Role.Should().Be("User");
        _users.Verify(r => r.SaveRefreshTokenAsync(42, It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_RejectsUnknownEmail()
    {
        _users.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var act = () => _sut.LoginAsync(new LoginRequest("nope@x.com", "password"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_RejectsBadPassword()
    {
        var (hash, salt) = JwtService.HashPassword("real-password");
        _users.Setup(r => r.GetByEmailAsync("u@x.com")).ReturnsAsync(new User
        {
            Id = 1, Username = "u", Email = "u@x.com", PasswordHash = hash, PasswordSalt = salt, Role = "User"
        });

        var act = () => _sut.LoginAsync(new LoginRequest("u@x.com", "wrong-password"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_HappyPath_ReturnsTokens()
    {
        var (hash, salt) = JwtService.HashPassword("good-password");
        _users.Setup(r => r.GetByEmailAsync("u@x.com")).ReturnsAsync(new User
        {
            Id = 7, Username = "u", Email = "u@x.com", PasswordHash = hash, PasswordSalt = salt, Role = "Admin"
        });

        var result = await _sut.LoginAsync(new LoginRequest("u@x.com", "good-password"));

        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.Role.Should().Be("Admin");
        _users.Verify(r => r.SaveRefreshTokenAsync(7, It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_RejectsUnknownToken()
    {
        _users.Setup(r => r.GetRefreshTokenAsync("bad")).ReturnsAsync(((int, bool, DateTime)?)null);

        var act = () => _sut.RefreshTokenAsync(new RefreshTokenRequest("bad"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task RefreshTokenAsync_RejectsExpiredToken()
    {
        _users.Setup(r => r.GetRefreshTokenAsync("old"))
              .ReturnsAsync((1, false, DateTime.UtcNow.AddDays(-1)));

        var act = () => _sut.RefreshTokenAsync(new RefreshTokenRequest("old"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*expired*");
    }

    [Fact]
    public async Task RefreshTokenAsync_RejectsRevokedToken()
    {
        _users.Setup(r => r.GetRefreshTokenAsync("revoked"))
              .ReturnsAsync((1, true, DateTime.UtcNow.AddDays(1)));

        var act = () => _sut.RefreshTokenAsync(new RefreshTokenRequest("revoked"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task RefreshTokenAsync_HappyPath_RevokesOldAndIssuesNew()
    {
        _users.Setup(r => r.GetRefreshTokenAsync("valid"))
              .ReturnsAsync((9, false, DateTime.UtcNow.AddDays(3)));
        _users.Setup(r => r.GetByIdAsync(9)).ReturnsAsync(new User
        {
            Id = 9, Username = "u", Email = "u@x.com", Role = "User"
        });

        var result = await _sut.RefreshTokenAsync(new RefreshTokenRequest("valid"));

        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        _users.Verify(r => r.RevokeRefreshTokenAsync("valid"), Times.Once);
        _users.Verify(r => r.SaveRefreshTokenAsync(9, It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
    }
}
