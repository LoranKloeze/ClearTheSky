using Backend.Web.Services.Interfaces;
using FishyFlip;
using FishyFlip.Models;

namespace Backend.Web.Services;

public class FishyFlipService(ILogger<FishyFlipService> logger) : IFishyFlipService
{
    public async Task<Session?> AuthenticateWithPasswordAsync(string handle, string password)
    {
        var protocol = new ATProtocolBuilder()
            .WithLogger(logger)
            .Build();

        var session = await protocol.AuthenticateWithPasswordAsync(handle, password);
        return session;
    }
}