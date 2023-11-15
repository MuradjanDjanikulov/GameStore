using Api.Models;
using DataAccess.Entity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Utils
{
    public class AppDbInitializer
    {
        public static async Task SeedRolesToDb(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                {
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
                }

                if (!await roleManager.RoleExistsAsync(UserRoles.Manager)) 
                {
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Manager));
                }

                if (!await roleManager.RoleExistsAsync(UserRoles.User))
                {
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
                }


                var adminUser = await userManager.FindByNameAsync("administrator");


                if (adminUser == null)
                {

                    var user = new AppUser
                    {
                        SecurityStamp = Guid.NewGuid().ToString(),
                        UserName = "admin",
                        Email = "admin@example.com",
                        FirstName = "administrator",
                        LastName = "administrator",
                    };

                    var result = await userManager.CreateAsync(user, "Admin123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, UserRoles.Admin);

                    }
                }

                var simpleUser = await userManager.FindByNameAsync("user");

                if (simpleUser == null)
                {

                    var user = new AppUser
                    {
                        SecurityStamp = Guid.NewGuid().ToString(),
                        UserName = "user",
                        Email = "user@example.com",
                        FirstName = "user",
                        LastName = "user",
                    };

                    var result = await userManager.CreateAsync(user, "User123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, UserRoles.User);

                    }
                }

                var managerUser = await userManager.FindByNameAsync("manager");

                if (managerUser == null)
                {

                    var user = new AppUser
                    {
                        SecurityStamp = Guid.NewGuid().ToString(),
                        UserName = "manager",
                        Email = "manager@example.com",
                        FirstName = "manager",
                        LastName = "manager",
                    };

                    var result = await userManager.CreateAsync(user, "Manager123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, UserRoles.Manager);

                    }
                }


            }
        }
    }
}
