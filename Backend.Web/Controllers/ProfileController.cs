using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Backend.Web.Data;
using Backend.Web.Services.Interfaces;
using FishyFlip;
using FishyFlip.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class ProfileController(
    ApplicationDbContext context, 
    ILogger<ProfileController> logger,
    IEncryptionService encryptionService)  : ControllerBase
{
    
    [HttpGet]
    [Authorize]
    public async Task<Results<Ok<FeedProfile>, UnauthorizedHttpResult>> GetProfileAsync()
    {
        var accessToken =  Request.Headers.Authorization.ToString().Split(" ")[1];
        var claimNameIdentifier = 
            new JwtSecurityTokenHandler().ReadJwtToken(accessToken)
                .Claims.First(claim => claim.Type == "nameid").Value;

        var user = await context.Users.FindAsync(claimNameIdentifier);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        var protocol = new ATProtocolBuilder()
            .WithLogger(logger)
            .Build();
        
        if (user.FishyFlipSessionEncrypted == null)
        {
            return TypedResults.Unauthorized();
        }

        var sessionJson = encryptionService.Decrypt(user.FishyFlipSessionEncrypted);
        var session = JsonSerializer.Deserialize<Session>(sessionJson);
        if (session == null)
        {
            return TypedResults.Unauthorized();
        }

        var authSession = new AuthSession(session);
        await protocol.AuthenticateWithPasswordSessionAsync(authSession);
        var profileResult = await protocol.Actor.GetProfileAsync(session.Did);
        var profile = profileResult.AsT0;
        
        user.FishyFlipSessionEncrypted = encryptionService.Encrypt(JsonSerializer.Serialize(protocol.Session));
        await context.SaveChangesAsync();
        
        return TypedResults.Ok(profile);
    }
    
}