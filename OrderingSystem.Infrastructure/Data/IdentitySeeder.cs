using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrderingSystem.Domain.Entities;

namespace OrderingSystem.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager =
            serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        var userManager =
            serviceProvider.GetRequiredService<UserManager<Customer>>();

        string[] roles =
        {
            "Admin",
            "Customer"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole<int>(role));

                if (!result.Succeeded)
                {
                    throw new Exception(
                        $"Failed to create role {role}: " +
                        string.Join(", ", result.Errors.Select(x => x.Description)));
                }
            }
        }

        const string adminEmail = "admin@orderingsystem.com";
        const string adminPassword = "Admin@12345";

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new Customer
            {
                UserName = adminEmail,
                Email = adminEmail,
                Name = "System Administrator",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(
                admin,
                adminPassword);

            if (!result.Succeeded)
            {
                throw new Exception(
                    "Failed to create admin user: " +
                    string.Join(", ", result.Errors.Select(x => x.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var result = await userManager.AddToRoleAsync(
                admin,
                "Admin");

            if (!result.Succeeded)
            {
                throw new Exception(
                    "Failed to assign Admin role: " +
                    string.Join(", ", result.Errors.Select(x => x.Description)));
            }
        }
    }
}