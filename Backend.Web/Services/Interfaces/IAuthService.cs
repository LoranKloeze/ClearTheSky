using Backend.Web.Models;

namespace Backend.Web.Services.Interfaces;

public interface IAuthService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}