using ClothesShop.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ClothesShop.Repository.DbSeed
{
    public static class SeedIdentityData
    {
        private const string adminUser = "Admin";
        private const string adminPass = "Secret123+";

        public static async Task<bool> EnsurePopulated(IApplicationBuilder app)
        {
            IdentityContext context = app.ApplicationServices
                .CreateScope().ServiceProvider
                .GetRequiredService<IdentityContext>();

            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }

            RoleManager<IdentityRole> roleManager = app.ApplicationServices
                .CreateScope().ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();


            IdentityRole role = await roleManager.FindByNameAsync("Administrator");
            if(role == null)
            {
                await roleManager.CreateAsync(new IdentityRole() {
                    Name = "Administrator",                
                });
            }

            role = await roleManager.FindByNameAsync("Administrator");

            IdentityRole userRole = await roleManager.FindByNameAsync("User");
            if (userRole == null)
            {
                await roleManager.CreateAsync(new IdentityRole()
                {
                    Name = "User"
                });
            }


            UserManager<IdentityUser> userManager = app.ApplicationServices
                .CreateScope().ServiceProvider
                .GetRequiredService<UserManager<IdentityUser>>();

            IdentityUser user = await userManager.FindByNameAsync(adminUser);
            if (user == null)
            {
                user = new IdentityUser(adminUser);
                user.Email = "admin@test.com";
                user.PhoneNumber = "123-123";
                user.EmailConfirmed = true;

                var userResult = await userManager.CreateAsync(user, adminPass);

                if (!userResult.Succeeded)
                {
                    throw new System.Exception("Couldn't create Administrator account!");
                }

                if (!(await userManager.IsInRoleAsync(user, "Administrator")))
                {
                    var result = await userManager.AddToRoleAsync(user, "Administrator");
                    if (result.Succeeded)
                    {
                        Debug.WriteLine("Added user the Administrator role!");
                    }
                    else
                    {
                        throw new System.Exception("Couldn't assign role to Administrator account!");
                    }
                }
            }

            DatabaseContext userContext = app.ApplicationServices
                .CreateScope().ServiceProvider
                .GetRequiredService<DatabaseContext>();

            var existingProfile = await userContext.Profiles.FirstOrDefaultAsync(x => x.AccountId == user.Id);

            if (existingProfile == null)
            {
                var profile = new UserProfile()
                {
                    Name = user.UserName,
                    Email = user.Email,
                    AccountId = user.Id,
                    City = "",
                    Address = "",
                    State = "",
                    Zip = ""
                };

                await userContext.Profiles.AddAsync(profile);
                await userContext.SaveChangesAsync();
            }
            return true;
        }
    }
}
