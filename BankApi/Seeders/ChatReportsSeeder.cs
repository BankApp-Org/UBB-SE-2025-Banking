using BankApi.Data;
using Common.Models; // Assuming ChatReport model is in Common.Models
using Microsoft.EntityFrameworkCore; // Required for AnyAsync

namespace BankApi.Seeders
{
    public class ChatReportsSeeder(IConfiguration configuration, IServiceProvider serviceProvider) : RegularTableSeeder<ChatReport>(configuration, serviceProvider)
    {

        // Implemented the abstract method SeedDataAsync
        protected override async Task SeedDataAsync(ApiDbContext context)
        {
            if (await context.ChatReports.AnyAsync())
            {
                Console.WriteLine("ChatReports already exist, skipping seeding.");
                return;
            }

            // Ensure that the referenced Users (by CNP) exist.
            // This seeder should run after UsersSeeder.
            var userCnps = new[] { "1234567890123", "9876543210987", "2345678901234", "3456789012345", "4567890123456" };
            var existingUsersCnps = await context.Users.Where(u => userCnps.Contains(u.CNP)).Select(u => u.CNP).ToListAsync();

            var chatReportsToSeed = new List<ChatReport>();

            var rawData = new[]
            {
                new { ReportedUserCnp = "1234567890123", ReportedMessage = "This user sent inappropriate content." },
                new { ReportedUserCnp = "9876543210987", ReportedMessage = "Reported for spamming multiple messages." },
                new { ReportedUserCnp = "2345678901234", ReportedMessage = "This user violated chat guidelines." },
                new { ReportedUserCnp = "3456789012345", ReportedMessage = "Reported for offensive language." },
                new { ReportedUserCnp = "4567890123456", ReportedMessage = "User harassed another member." }
            };

            foreach (var data in rawData)
            {
                if (existingUsersCnps.Contains(data.ReportedUserCnp))
                {
                    chatReportsToSeed.Add(new ChatReport { ReportedUserCnp = data.ReportedUserCnp, ReportedMessage = data.ReportedMessage });
                }
                else
                {
                    Console.WriteLine($"Skipping ChatReport for UserCnp: {data.ReportedUserCnp} as user does not exist.");
                }
            }

            if (chatReportsToSeed.Count != 0)
            {
                await context.ChatReports.AddRangeAsync(chatReportsToSeed);
            }
            else
            {
                Console.WriteLine("No valid ChatReports to seed due to missing related users.");
            }
        }
    }
}