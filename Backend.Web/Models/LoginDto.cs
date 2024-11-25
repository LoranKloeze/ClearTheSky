// ReSharper disable PropertyCanBeMadeInitOnly.Global
namespace Backend.Web.Models;

public class LoginDto
{
    public required string Handle { get; set; }
    public required string AppPassword { get; set; }
}