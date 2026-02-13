using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Shared.Enums;
using Microsoft.AspNetCore.Identity;

namespace DidWeFeedTheCatToday.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (!context.Users.Any())
            {
                var hasher = new PasswordHasher<User>();

                var admin = new User
                {
                    Username = "admin",
                    Role = Roles.Admin,
                };

                admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

                context.Users.Add(admin);
                await context.SaveChangesAsync();
            }

            if (!context.Pets.Any())
            {
                var pet = new Pet
                { 
                    Name = "Meowstarion",
                    Age = 10,
                    AdditionalInformation = "Don't get fooled by his acting!"
                };

                context.Pets.Add(pet);
                await context.SaveChangesAsync();

                var feedings = new List<Feeding>
                {
                    new() {PetId = pet.Id, FeedingTime = DateTime.UtcNow.AddHours(-1)},
                    new() {PetId = pet.Id, FeedingTime = DateTime.UtcNow.AddHours(-2)},
                    new() {PetId = pet.Id, FeedingTime = DateTime.UtcNow.AddHours(-3)}
                };

                context.Feedings.AddRange(feedings);
                await context.SaveChangesAsync();
            }
        }
    }
}
