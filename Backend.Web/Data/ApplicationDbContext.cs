using Backend.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }
    public DbSet<RefreshToken> RefreshTokens { get; init; }
    
}