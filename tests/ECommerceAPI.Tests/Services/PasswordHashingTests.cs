using ECommerceAPI.Services;
using FluentAssertions;
using Xunit;

namespace ECommerceAPI.Tests.Services;

public class PasswordHashingTests
{
    [Fact]
    public void Hash_ProducesDifferentSaltsForSamePassword()
    {
        var (h1, s1) = JwtService.HashPassword("password123");
        var (h2, s2) = JwtService.HashPassword("password123");

        s1.Should().NotBe(s2, "salt must be random per hash");
        h1.Should().NotBe(h2, "hash must differ when salt differs");
    }

    [Fact]
    public void Verify_ReturnsTrueForCorrectPassword()
    {
        var (hash, salt) = JwtService.HashPassword("correct horse battery staple");
        JwtService.VerifyPassword("correct horse battery staple", hash, salt).Should().BeTrue();
    }

    [Theory]
    [InlineData("password123", "password124")]
    [InlineData("password123", "")]
    [InlineData("password123", "PASSWORD123")]
    public void Verify_ReturnsFalseForWrongPassword(string original, string attempt)
    {
        var (hash, salt) = JwtService.HashPassword(original);
        JwtService.VerifyPassword(attempt, hash, salt).Should().BeFalse();
    }

    [Fact]
    public void Verify_ReturnsFalseWhenSaltIsTampered()
    {
        var (hash, salt) = JwtService.HashPassword("password123");
        var tamperedSalt = Convert.ToBase64String(new byte[64]); // all zeros
        JwtService.VerifyPassword("password123", hash, tamperedSalt).Should().BeFalse();
    }
}
