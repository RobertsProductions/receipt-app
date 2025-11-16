using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MyApi.Models;
using MyApi.Services;
using Xunit;

namespace MyApi.Tests.Services;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public TokenServiceTests()
    {
        // Arrange - Setup JWT settings for testing
        _jwtSettings = new JwtSettings
        {
            Secret = "ThisIsAVeryLongSecretKeyForTestingPurposesWithAtLeast256BitsOfLength12345",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryInMinutes = 60
        };

        var options = Options.Create(_jwtSettings);
        _tokenService = new TokenService(options);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsValidJwtToken()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var token = _tokenService.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Issuer.Should().Be(_jwtSettings.Issuer);
        jwtToken.Audiences.Should().Contain(_jwtSettings.Audience);
        jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateToken_ContainsUserClaims()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var token = _tokenService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == user.UserName);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.GivenName && c.Value == user.FirstName);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Surname && c.Value == user.LastName);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public void GenerateToken_WithoutFirstName_ExcludesGivenNameClaim()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser",
            Email = "test@example.com",
            FirstName = null,
            LastName = "User"
        };

        // Act
        var token = _tokenService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Claims.Should().NotContain(c => c.Type == ClaimTypes.GivenName);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Surname && c.Value == user.LastName);
    }

    [Fact]
    public void GenerateToken_WithoutLastName_ExcludesSurnameClaim()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = null
        };

        // Act
        var token = _tokenService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.GivenName && c.Value == user.FirstName);
        jwtToken.Claims.Should().NotContain(c => c.Type == ClaimTypes.Surname);
    }

    [Fact]
    public void GenerateToken_CreatesUniqueJtiForEachToken()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser",
            Email = "test@example.com"
        };

        // Act
        var token1 = _tokenService.GenerateToken(user);
        var token2 = _tokenService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken1 = handler.ReadJwtToken(token1);
        var jwtToken2 = handler.ReadJwtToken(token2);
        
        var jti1 = jwtToken1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = jwtToken2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        
        jti1.Should().NotBe(jti2);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsBase64String()
    {
        // Act
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
        
        // Should be valid base64 (this won't throw if valid)
        var bytes = Convert.FromBase64String(refreshToken);
        bytes.Length.Should().Be(64);
    }

    [Fact]
    public void GenerateRefreshToken_CreatesUniqueTokens()
    {
        // Act
        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithValidToken_ReturnsPrincipal()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser",
            Email = "test@example.com"
        };
        var token = _tokenService.GenerateToken(user);

        // Act
        var principal = _tokenService.GetPrincipalFromExpiredToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id);
        principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        var principal = _tokenService.GetPrincipalFromExpiredToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithWrongSignature_ReturnsNull()
    {
        // Arrange - Generate token with different settings
        var differentSettings = new JwtSettings
        {
            Secret = "DifferentSecretKeyThatWillCauseSignatureMismatchForTestingPurposes123",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryInMinutes = 60
        };
        var differentService = new TokenService(Options.Create(differentSettings));
        
        var user = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser",
            Email = "test@example.com"
        };
        var tokenWithDifferentSignature = differentService.GenerateToken(user);

        // Act - Try to validate with original service
        var principal = _tokenService.GetPrincipalFromExpiredToken(tokenWithDifferentSignature);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithWrongIssuer_ReturnsNull()
    {
        // Arrange - Generate token with different issuer
        var differentSettings = new JwtSettings
        {
            Secret = _jwtSettings.Secret, // Same secret
            Issuer = "DifferentIssuer",   // Different issuer
            Audience = _jwtSettings.Audience,
            ExpiryInMinutes = 60
        };
        var differentService = new TokenService(Options.Create(differentSettings));
        
        var user = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser",
            Email = "test@example.com"
        };
        var tokenWithDifferentIssuer = differentService.GenerateToken(user);

        // Act - Try to validate with original service
        var principal = _tokenService.GetPrincipalFromExpiredToken(tokenWithDifferentIssuer);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_AcceptsExpiredTokens()
    {
        // Arrange - Create settings with very short expiry
        var shortExpirySettings = new JwtSettings
        {
            Secret = _jwtSettings.Secret,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            ExpiryInMinutes = -1 // Already expired
        };
        var shortExpiryService = new TokenService(Options.Create(shortExpirySettings));
        
        var user = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser",
            Email = "test@example.com"
        };
        var expiredToken = shortExpiryService.GenerateToken(user);

        // Act - Should still validate because lifetime validation is disabled
        var principal = _tokenService.GetPrincipalFromExpiredToken(expiredToken);

        // Assert
        principal.Should().NotBeNull();
        principal!.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id);
    }
}
