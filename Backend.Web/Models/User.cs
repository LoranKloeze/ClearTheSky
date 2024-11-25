using System.ComponentModel.DataAnnotations;

namespace Backend.Web.Models;

public class User
{
    [StringLength(1024)]
    [Key]
    public required string BlueskyDid { get; init; }
    
    [StringLength(20480)]
    public string? FishyFlipSessionEncrypted { get; set; }
    
    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<RefreshToken> RefreshTokens { get; init; } = [];
}