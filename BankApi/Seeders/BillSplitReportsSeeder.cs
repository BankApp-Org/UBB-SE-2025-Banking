using BankApi.Data;
using Common.Models; // Assuming BillSplitReport model is in Common.Models
using Microsoft.EntityFrameworkCore; // Required for AnyAsync

namespace BankApi.Seeders
{
    public class BillSplitReportsSeeder(IConfiguration configuration, IServiceProvider serviceProvider) : RegularTableSeeder<BillSplitReport>(configuration, serviceProvider)
    {

        // Implemented the abstract method SeedDataAsync
        protected override async Task SeedDataAsync(ApiDbContext context)
        {
            if (await context.BillSplitReports.AnyAsync())
            {
                Console.WriteLine("BillSplitReports already exist, skipping seeding.");
                return;
            }

            // Ensure that the referenced Users (by CNP) exist.
            // This seeder should run after UsersSeeder.
            var userCnps = new[]
            {
                "1234567890123", "9876543210987", "2345678901234",
                "3456789012345", "4567890123456"
            };
            var existingUsersCnps = await context.Users.Where(u => userCnps.Contains(u.CNP)).Select(u => u.CNP).ToListAsync();

            var reportsToSeed = new List<BillSplitReport>();

            var rawData = new[]
            {
                new { ReportedCnp = "1234567890123", ReportingCnp = "9876543210987", Date = new DateTime(2025, 4, 1), Share = 150.00m },
                new { ReportedCnp = "2345678901234", ReportingCnp = "3456789012345", Date = new DateTime(2025, 3, 15), Share = 90.25m },
                new { ReportedCnp = "9876543210987", ReportingCnp = "1234567890123", Date = new DateTime(2025, 2, 20), Share = 200.50m },
                new { ReportedCnp = "3456789012345", ReportingCnp = "4567890123456", Date = new DateTime(2025, 1, 10), Share = 75.75m },
                new { ReportedCnp = "4567890123456", ReportingCnp = "2345678901234", Date = new DateTime(2025, 5, 5), Share = 180.00m }
            };

            foreach (var data in rawData)
            {
                if (existingUsersCnps.Contains(data.ReportedCnp) && existingUsersCnps.Contains(data.ReportingCnp))
                {
                    reportsToSeed.Add(new BillSplitReport
                    {
                        ReportedUserCnp = data.ReportedCnp,
                        ReportingUserCnp = data.ReportingCnp,
                        DateOfTransaction = data.Date,
                        BillShare = data.Share
                    });
                }
                else
                {
                    Console.WriteLine($"Skipping BillSplitReport for Reported: {data.ReportedCnp}, Reporting: {data.ReportingCnp} as one or both users do not exist.");
                }
            }

            if (reportsToSeed.Count != 0)
            {
                await context.BillSplitReports.AddRangeAsync(reportsToSeed);
            }
            else
            {
                Console.WriteLine("No valid BillSplitReports to seed due to missing related users.");
            }
        }
    }
}