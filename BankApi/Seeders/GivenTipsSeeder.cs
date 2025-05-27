using BankApi.Data;
using Common.Models; // Assuming GivenTip model is in Common.Models
using Microsoft.EntityFrameworkCore; // Required for AnyAsync and other EF Core methods
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankApi.Seeders
{
    // Updated to inherit from ForeignKeyTableSeeder (which is not generic anymore)
    public class GivenTipsSeeder(IConfiguration configuration, IServiceProvider serviceProvider) : ForeignKeyTableSeeder(configuration, serviceProvider)
    {

        // Implemented the abstract method SeedDataAsync
        protected override async Task SeedDataAsync(ApiDbContext context)
        {
            if (await context.GivenTips.AnyAsync())
            {
                Console.WriteLine("GivenTips already exist, skipping seeding.");
                return;
            }

            // These CNPs should correspond to User entities created by UsersSeeder
            var userCnpsToLink = new[] { "1234567890123", "9876543210987", "2345678901234", "3456789012345", "4567890123456" };
            var existingUsers = await context.Users
                                           .Where(u => userCnpsToLink.Contains(u.CNP))
                                           .ToListAsync();

            // These TipIds should correspond to Tip entities created by TipsSeeder
            // For simplicity, let's assume the first 5 tips are used. 
            // A more robust approach would be to fetch tips by a known property if Ids are not predictable.
            var existingTips = await context.Tips.Take(5).ToListAsync();

            if (existingUsers.Count < userCnpsToLink.Length || existingTips.Count < 5)
            {
                Console.WriteLine("Warning: Not all specified users or tips for GivenTipsSeeder were found. Seeding might be incomplete.");
            }

            var givenTipsToSeed = new List<GivenTip>();
            var dates = new[] { new DateTime(2025, 4, 1), new DateTime(2025, 3, 15), new DateTime(2025, 2, 20), new DateTime(2025, 1, 10), new DateTime(2025, 5, 5) };

            for (int i = 0; i < Math.Min(Math.Min(existingUsers.Count, existingTips.Count), dates.Length); i++)
            {
                var user = existingUsers[i];
                var tip = existingTips[i];

                givenTipsToSeed.Add(new GivenTip
                {
                    TipId = tip.Id,
                    UserCNP = user.CNP, // Set the UserCNP string property
                    User = user,        // Set the User navigation property
                    Tip = tip,          // Set the Tip navigation property
                    Date = dates[i]
                });
            }

            if (givenTipsToSeed.Count != 0)
            {
                await context.GivenTips.AddRangeAsync(givenTipsToSeed);
            }
            else
            {
                Console.WriteLine("No valid given tips to seed due to missing related user or tip entities.");
            }
        }
    }
}