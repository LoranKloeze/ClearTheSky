using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Web.Models;

[Index(nameof(TokenHash), IsUnique = true)]
public class RefreshToken
{
    public Guid Id { get; init; }
    
    [StringLength(2048)]
    public required string TokenHash  { get; init; }
    
    public required User User { get; init; }
    
    public required DateTime CreatedAt { get; init; }
}