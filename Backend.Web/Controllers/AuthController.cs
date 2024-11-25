using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Backend.Web.Data;
using Backend.Web.Dtos;
using Backend.Web.Models;
using Backend.Web.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(
    IAuthService authService,
    IEncryptionService encryptionService,
    IFishyFlipService fishyFlipService,
    ApplicationDbContext context
) : ControllerBase
{
    private const string RefreshTokenKey = "_refreshToken";
    private readonly TimeSpan _maxAgeRefreshToken = TimeSpan.FromDays(30);

    [HttpPost("login")]
    public async Task<Results<Ok<AuthLoginResult>, UnauthorizedHttpResult>> LoginAsync(LoginDto dto)
    {
        var session = await fishyFlipService.AuthenticateWithPasswordAsync(dto.Handle, dto.AppPassword);
        if (session is null)
        {
            return TypedResults.Unauthorized();
        }

        var user = await context.Users.FindAsync(session.Did.ToString());
        if (user == null)
        {
            user = new User
            {
                BlueskyDid = session.Did.ToString()
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }

        var sessionJson = JsonSerializer.Serialize(session);
        user.FishyFlipSessionEncrypted = encryptionService.Encrypt(sessionJson);

        var refreshToken = authService.GenerateRefreshToken();
        context.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = HashToken(refreshToken),
            User = user,
            CreatedAt = DateTime.Now.ToUniversalTime()
        });

        await context.SaveChangesAsync();

        Response.Cookies.Append(RefreshTokenKey, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict
        });
        return TypedResults.Ok(new AuthLoginResult
        {
            AccessToken = authService.GenerateAccessToken(user)
        });
    }

    [HttpPost("refresh")]
    public async Task<Results<Ok<AuthLoginResult>, UnauthorizedHttpResult>> RefreshAsync()
    {
        var refreshToken = Request.Cookies[RefreshTokenKey];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return TypedResults.Unauthorized();
        }

        var refreshTokenEntity = await context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == HashToken(refreshToken));
        if (refreshTokenEntity == null)
        {
            return TypedResults.Unauthorized();
        }

        var user = refreshTokenEntity.User;

        var tooOld = refreshTokenEntity.CreatedAt.Add(_maxAgeRefreshToken) < DateTime.Now.ToUniversalTime();
        if (tooOld)
        {
            context.RefreshTokens.Remove(refreshTokenEntity);
            await context.SaveChangesAsync();
            return TypedResults.Unauthorized();
        }
        
        
        context.RefreshTokens.Remove(refreshTokenEntity);

        var newRefreshToken = authService.GenerateRefreshToken();
        context.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = HashToken(newRefreshToken),
            User = user,
            CreatedAt = DateTime.Now.ToUniversalTime()
        });

        await context.SaveChangesAsync();

        Response.Cookies.Append(RefreshTokenKey, newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict
        });
        return TypedResults.Ok(new AuthLoginResult
        {
            AccessToken = authService.GenerateAccessToken(user)
        });
    }

    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }
}