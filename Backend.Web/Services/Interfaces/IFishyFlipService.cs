using FishyFlip.Models;

namespace Backend.Web.Services.Interfaces;

public interface IFishyFlipService
{
    Task<Session?> AuthenticateWithPasswordAsync(string handle, string password);
}