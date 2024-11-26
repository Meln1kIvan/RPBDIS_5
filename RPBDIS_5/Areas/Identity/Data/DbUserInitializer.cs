using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using RPBDIS_5.Areas.Identity.Models;

namespace RPBDIS_5.Areas.Identity.Data
{
    public static class DbUserInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string adminEmail = "admin@gmail.com";
            string adminName = "admin@gmail.com";
            string password = "_Aa123456";

            if (await roleManager.FindByNameAsync("admin") == null)
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole("admin"));
                if (!roleResult.Succeeded)
                {
                    throw new Exception("Failed to create 'admin' role.");
                }
            }
            if (await roleManager.FindByNameAsync("user") == null)
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole("user"));
                if (!roleResult.Succeeded)
                {
                    throw new Exception("Failed to create 'user' role.");
                }
            }

            if (await userManager.FindByNameAsync(adminEmail) == null)
            {
                ApplicationUser admin = new()
                {
                    Email = adminEmail,
                    UserName = adminName,
                    RegistrationDate = DateTime.Now
                };
                var userResult = await userManager.CreateAsync(admin, password);
                if (!userResult.Succeeded)
                {
                    throw new Exception("Failed to create admin user. Errors: " + string.Join(", ", userResult.Errors.Select(e => e.Description)));
                }

                var roleAssignmentResult = await userManager.AddToRoleAsync(admin, "admin");
                if (!roleAssignmentResult.Succeeded)
                {
                    throw new Exception("Failed to assign 'admin' role to the user.");
                }
            }
        }
    }
}
