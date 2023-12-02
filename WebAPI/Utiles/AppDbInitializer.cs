using DataAccess.Entity;
using DataAccess.Repository;
using Microsoft.AspNetCore.Identity;

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
                var gameRepository = serviceScope.ServiceProvider.GetRequiredService<IGameRepository>();

                var existingGame = await gameRepository.GetByName("Counter Strike 1.6");

                if (existingGame == null)
                {
                    var newGame = new Game
                    {
                        Name = "Counter Strike 1.6",
                        Description = "Counter Strike 1.6 Description",
                        Price = 250
                    };

                    var existingGenres = await gameRepository.GetGenres(new HashSet<int>(new int[] { 9, 15 }));

                    if (existingGenres != null) { newGame.Genres = new HashSet<Genre>(existingGenres); }

                    await gameRepository.Create(newGame);
                }
                existingGame = await gameRepository.GetByName("Counter Strike GO");

                if (existingGame == null)
                {
                    var newGame = new Game
                    {
                        Name = "Counter Strike GO",
                        Description = "Counter Strike GO Description",
                        Price = 350
                    };

                    var existingGenres = await gameRepository.GetGenres(new HashSet<int>(new int[] { 9, 15 }));

                    if (existingGenres != null) { newGame.Genres = new HashSet<Genre>(existingGenres); }

                    await gameRepository.Create(newGame);
                }

                existingGame = await gameRepository.GetByName("Need for Speed: Underground");

                if (existingGame == null)
                {
                    var newGame = new Game
                    {
                        Name = "Need for Speed: Underground",
                        Description = "Need for Speed: Underground Description",
                        Price = 300
                    };

                    var existingGenres = await gameRepository.GetGenres(new HashSet<int>(new int[] { 8, 15 }));

                    if (existingGenres != null) { newGame.Genres = new HashSet<Genre>(existingGenres); }

                    await gameRepository.Create(newGame);
                }


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
                        TokenSign = Guid.NewGuid().ToString(),
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
                        TokenSign = Guid.NewGuid().ToString(),
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
                        TokenSign = Guid.NewGuid().ToString(),
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
