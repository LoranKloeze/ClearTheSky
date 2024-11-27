// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Backend.Web.Dtos;

public class GetProfileResult
{
    public required string Did { get; set; }
    public required string Handle { get; set; }
    public required string DisplayName { get; set; }
    public required string Description { get; set; }
    public required string Avatar { get; set; }
    public int FollowsCount { get; set; }
    public int FollowersCount { get; set; }
    public int PostsCount { get; set; }
}