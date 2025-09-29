using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PreschoolManagement.Models;

namespace PreschoolManagement.Data
{
    public static class SeedData
    {
        private static readonly string[] Roles = new[] { "Admin", "Teacher", "Parent", "Staff" };

        public static async Task InitializeAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await ctx.Database.MigrateAsync();

            foreach (var r in Roles)
            {
                if (!await roleManager.RoleExistsAsync(r))
                    await roleManager.CreateAsync(new IdentityRole(r));
            }

            var adminEmail = "admin@preschool.local";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Administrator"
                };
                await userManager.CreateAsync(admin, "Admin@123"); // đổi mật khẩu sau
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
