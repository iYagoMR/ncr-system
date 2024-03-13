using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Haver.Data
{
    public class ApplicationDBInitializer
    {
        public static async void Seed(IApplicationBuilder applicastionBuilder)
        {
            ApplicationDbContext context = applicastionBuilder.ApplicationServices.CreateScope()
                .ServiceProvider.GetRequiredService<ApplicationDbContext>();
            try
            {
                context.Database.Migrate();

                var RoleManager = applicastionBuilder.ApplicationServices.CreateScope()
                    .ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                string[] roleNames = new string[] { "Admin", "Engineering", "Procurement", "Operations Manager", "Quality Inspector" };
                IdentityResult roleResult;
                foreach (var roleName in roleNames)
                {
                    var roleExist = await RoleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }
                var userManager = applicastionBuilder.ApplicationServices.CreateScope()
                    .ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                if (userManager.FindByEmailAsync("iago.romao5@gmail.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "iago.romao5@gmail.com",
                        Email = "iago.romao5@gmail.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Admin").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("super@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "super@outlook.com",
                        Email = "super@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Engineering").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("user@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "user@outlook.com",
                        Email = "user@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Procurement").Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.GetBaseException().Message);
            }
        }
    }
}
