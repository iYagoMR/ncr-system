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
                string[] roleNames = new string[] { "Admin", "Engineer", "Procurement", "Operations Manager", "Quality Inspector" };
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
                if (userManager.FindByEmailAsync("qualityinsp@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "qualityinsp@outlook.com",
                        Email = "qualityinsp@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Quality Inspector").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("engineer@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "engineer@outlook.com",
                        Email = "engineer@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Engineer").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("opmanager@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "opmanager@outlook.com",
                        Email = "opmanager@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Operations Manager").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("procurement@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "procurement@outlook.com",
                        Email = "procurement@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Procurement").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("admin@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "admin@outlook.com",
                        Email = "admin@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Admin").Wait();
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
