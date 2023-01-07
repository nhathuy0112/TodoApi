using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Models.Identity;

namespace TodoApi;

public class TodoDbContext : IdentityDbContext<User>
{
    public TodoDbContext(DbContextOptions options) : base(options)
    {
    }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    
}