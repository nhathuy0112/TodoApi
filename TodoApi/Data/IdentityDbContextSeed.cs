using Microsoft.AspNetCore.Identity;
using TodoApi.Models.Identity;

namespace TodoApi.Data;

public class IdentityDbContextSeed
{
    public static async Task SeedRoleAsync(RoleManager<IdentityRole> roleManager)
    {
        if (!roleManager.Roles.Any())
        {
            await roleManager.CreateAsync(new IdentityRole(Role.ADMIN.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Role.USER.ToString()));
        }
    }
}