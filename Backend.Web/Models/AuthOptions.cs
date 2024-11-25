namespace Backend.Web.Models;

public class AuthOptions
{
    public const string Auth = "Auth";

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public string PrivateKey { get; set; } = string.Empty;
}