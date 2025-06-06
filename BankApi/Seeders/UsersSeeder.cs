using BankApi.Data;
using Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Seeders
{
    public class UsersSeeder : TableSeeder
    {
        public UsersSeeder(IConfiguration configuration, IServiceProvider serviceProvider)
            : base(configuration, serviceProvider)
        {
        }

        public override async Task SeedAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

            // Check if users already exist
            if (dbContext.Users.Any())
            {
                Console.WriteLine("Users already exist, skipping seeding.");
                return;
            }

            // Ensure User role exists
            string defaultRole = "User";
            if (!await roleManager.RoleExistsAsync(defaultRole))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(defaultRole));
                Console.WriteLine($"Created '{defaultRole}' role.");
            }

            // Ensure Admin role exists
            string adminRole = "Admin";
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(adminRole));
                Console.WriteLine($"Created '{adminRole}' role.");
            }

            var users = new[]
            {
                new User { CNP = "1234567890123", UserName = "user1_cnp123", FirstName = "Victor", LastName = "Pascu", Email = "victor.pascu@example.com", PhoneNumber = "1234567890", Description = "User 1 description", Image = "img1.jpg", IsHidden = false, GemBalance = 100, NumberOfOffenses = 0, RiskScore = 10, ROI = 5.5m, CreditScore = 700, Birthday = new DateTime(1990, 1, 1), ZodiacSign = "Aries", ZodiacAttribute = "Fire", NumberOfBillSharesPaid = 10, Income = 50000, Balance = 10000m, EmailConfirmed = true, NormalizedUserName = "USER1_CNP123", NormalizedEmail = "VICTOR.PASCU@EXAMPLE.COM" },
                new User { CNP = "9876543210987", UserName = "user2_cnp987", FirstName = "Mihai", LastName = "Popescu", Email = "mihai.popescu@example.com", PhoneNumber = "0987654321", Description = "User 2 description", Image = "img2.jpg", IsHidden = false, GemBalance = 200, NumberOfOffenses = 1, RiskScore = 20, ROI = 6.5m, CreditScore = 650, Birthday = new DateTime(1985, 5, 5), ZodiacSign = "Taurus", ZodiacAttribute = "Earth", NumberOfBillSharesPaid = 5, Income = 60000, Balance = 15000m, EmailConfirmed = true, NormalizedUserName = "USER2_CNP987", NormalizedEmail = "MIHAI.POPESCU@EXAMPLE.COM" },
                new User { CNP = "2345678901234", UserName = "user3_cnp234", FirstName = "Alina", LastName = "Georgescu", Email = "alina.georgescu@example.com", PhoneNumber = "1122334455", Description = "User 3 description", Image = "img3.jpg", IsHidden = true, GemBalance = 50, NumberOfOffenses = 0, RiskScore = 5, ROI = 4.0m, CreditScore = 750, Birthday = new DateTime(1995, 10, 10), ZodiacSign = "Gemini", ZodiacAttribute = "Air", NumberOfBillSharesPaid = 20, Income = 40000, Balance = 5000m, EmailConfirmed = true, NormalizedUserName = "USER3_CNP234", NormalizedEmail = "ALINA.GEORGESCU@EXAMPLE.COM" },
                new User { CNP = "3456789012345", UserName = "user4_cnp345", FirstName = "Cristian", LastName = "Dumitrescu", Email = "cristian.dumitrescu@example.com", PhoneNumber = "5544332211", Description = "User 4 description", Image = "img4.jpg", IsHidden = false, GemBalance = 150, NumberOfOffenses = 2, RiskScore = 30, ROI = 7.0m, CreditScore = 600, Birthday = new DateTime(1980, 3, 3), ZodiacSign = "Cancer", ZodiacAttribute = "Water", NumberOfBillSharesPaid = 15, Income = 70000, Balance = 20000m, EmailConfirmed = true, NormalizedUserName = "USER4_CNP345", NormalizedEmail = "CRISTIAN.DUMITRESCU@EXAMPLE.COM" },
                new User { CNP = "4567890123456", UserName = "user5_cnp456", FirstName = "Elena", LastName = "Ionescu", Email = "elena.ionescu@example.com", PhoneNumber = "6677889900", Description = "User 5 description", Image = "img5.jpg", IsHidden = false, GemBalance = 250, NumberOfOffenses = 0, RiskScore = 15, ROI = 5.0m, CreditScore = 800, Birthday = new DateTime(2000, 7, 7), ZodiacSign = "Leo", ZodiacAttribute = "Fire", NumberOfBillSharesPaid = 25, Income = 45000, Balance = 12000m, EmailConfirmed = true, NormalizedUserName = "USER5_CNP456", NormalizedEmail = "ELENA.IONESCU@EXAMPLE.COM" }
            };

            foreach (var user in users)
            {
                // Check if user with this CNP or UserName already exists to be safe
                if (await userManager.FindByNameAsync(user.UserName) == null && await dbContext.Users.AllAsync(u => u.CNP != user.CNP))
                {
                    var result = await userManager.CreateAsync(user, "Password123!"); // Use a default password
                    if (result.Succeeded)
                    {
                        // Assign User role by default
                        await userManager.AddToRoleAsync(user, defaultRole);

                        // Make the first user an admin as well
                        if (user.UserName == "user1_cnp123")
                        {
                            await userManager.AddToRoleAsync(user, adminRole);
                            Console.WriteLine($"User {user.UserName} assigned Admin role");
                        }

                        Console.WriteLine($"User {user.UserName} created successfully");
                    }
                    else
                    {
                        Console.Error.WriteLine($"Failed to create user {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }
    }
}