using System.Security.Cryptography;
using System.Text;
using Backend.Tests._Support;
using Backend.Web.Controllers;
using Backend.Web.Data;
using Backend.Web.Dtos;
using Backend.Web.Models;
using Backend.Web.Services.Interfaces;
using FishyFlip.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Backend.Tests.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private ApplicationDbContext _context;
    private IAuthService _authService;
    private IEncryptionService _encryptionService;
    private IFishyFlipService _fishyFlipService;
    private AuthController _controller;
    private User _user;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _user = new User { BlueskyDid = "did:fishyflip:123" };
        _context.Users.Add(_user);
        _context.SaveChanges();

        _authService = Substitute.For<IAuthService>();
        _encryptionService = Substitute.For<IEncryptionService>();
        _fishyFlipService = Substitute.For<IFishyFlipService>();

        _controller = new AuthController(
            _authService,
            _encryptionService,
            _fishyFlipService,
            _context
        )
        {
            ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task LoginAsync_ReturnsUnauthorized_WhenAuthenticationFails()
    {
        // Arrange
        var dto = new LoginDto { Handle = "testUser", AppPassword = "wrongPassword" };
        _fishyFlipService.AuthenticateWithPasswordAsync(dto.Handle, dto.AppPassword).Returns((Session?)null);

        // Act
        var result = await _controller.LoginAsync(dto);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<UnauthorizedHttpResult>());
    }

    [Test]
    public async Task LoginAsync_ReturnsTokens_WhenAuthenticationSucceeds()
    {
        // Arrange
        var dto = new LoginDto { Handle = "testUser", AppPassword = "goodpassword" };
        var session = BuildSession();
        _fishyFlipService.AuthenticateWithPasswordAsync(dto.Handle, dto.AppPassword).Returns(session);
        _authService.GenerateAccessToken(_user).Returns("anAccessToken");
        _authService.GenerateRefreshToken().Returns("aRefreshToken");

        // Act
        var result = await _controller.LoginAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<Ok<AuthLoginResult>>());
            var okResult = (Ok<AuthLoginResult>)result.Result;
            Assert.That(okResult.Value!.AccessToken, Is.Not.Null);
            Assert.That(okResult.Value!.AccessToken, Is.EqualTo("anAccessToken"));
            Assert.That(GetCookie( _controller.Response, "_refreshToken"), 
                Is.EqualTo("aRefreshToken"));
        });
    }

    [Test]
    public async Task RefreshAsync_ReturnsTokens_WhenRefreshTokenIsValid()
    {
        // Arrange
        const string refreshToken = "myRefreshToken";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        var base64Hash = Convert.ToBase64String(hash);
        _context.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = base64Hash,
            User = _user,
            CreatedAt = DateTime.Now.ToUniversalTime()
        });
        await _context.SaveChangesAsync();
        
        _authService.GenerateAccessToken(_user).Returns("aNewAccessToken");
        _authService.GenerateRefreshToken().Returns("aNewRefreshToken");
        
        var cookies = new MockRequestCookieCollection { { "_refreshToken", refreshToken} };
        _controller.Request.Cookies = cookies;
        
        // Act
        var result = await _controller.RefreshAsync();
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<Ok<AuthLoginResult>>());
            var okResult = (Ok<AuthLoginResult>)result.Result;
            Assert.That(okResult.Value!.AccessToken, Is.Not.Null);
            Assert.That(okResult.Value!.AccessToken, Is.EqualTo("aNewAccessToken"));
            Assert.That(GetCookie( _controller.Response, "_refreshToken"), 
                Is.EqualTo("aNewRefreshToken"));
        });
    }
    
    [Test]
    public async Task RefreshAsync_ReturnsUnauthorized_WhenRefreshTokenIsTooOld()
    {
        // Arrange
        const string refreshToken = "myRefreshToken";
        const int tokenAgeInDays = 31;
        
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        var base64Hash = Convert.ToBase64String(hash);
        _context.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = base64Hash,
            User = _user,
            CreatedAt = DateTime.Now.ToUniversalTime().AddDays(-tokenAgeInDays)
        });
        await _context.SaveChangesAsync();
        
        var cookies = new MockRequestCookieCollection { { "_refreshToken", refreshToken} };
        _controller.Request.Cookies = cookies;
        
        // Act
        var result = await _controller.RefreshAsync();
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<UnauthorizedHttpResult>());
    }
    
    [Test]
    public async Task RefreshAsync_ReturnsUnauthorized_WhenRefreshTokenIsNotFound()
    {
        // Arrange
        const string refreshToken = "nonExistingToken";
        
        var cookies = new MockRequestCookieCollection { { "_refreshToken", refreshToken} };
        _controller.Request.Cookies = cookies;
        
        // Act
        var result = await _controller.RefreshAsync();
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<UnauthorizedHttpResult>());
    }

    private static Session BuildSession()
    {
        return new Session(
            new ATDid("did:fishyflip:123"),
            null,
            new ATHandle("handle"),
            null,
            "accessJwt",
            "refreshJwt"
        );
    }

    private static string GetCookie(HttpResponse response, string keyName)
    {
        var cookie = response.Headers.SetCookie.First();
        if (cookie == null)
        {
            return "";
        }
        var cookieParts = cookie.Split(';');
        foreach (var part in cookieParts)
        {
            var keyValue = part.Split('=');
            if (keyValue[0] == keyName)
            {
                return keyValue[1];
            }
        }

        return "";
    }
}