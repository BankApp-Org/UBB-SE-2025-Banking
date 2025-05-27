using BankApi.Data;
using Common.Models;

namespace BankApi.Seeders
{
    public class ActivityLogsSeeder(IConfiguration configuration, IServiceProvider serviceProvider) : RegularTableSeeder<ActivityLog>(configuration, serviceProvider)
    {

        // Implemented the abstract method SeedDataAsync
        protected override async Task SeedDataAsync(ApiDbContext context)
        {
            if (context.ActivityLogs.Any())
            {
                Console.WriteLine("ActivityLogs already exist, skipping seeding.");
                return;
            }

            var activityLogs = new[]
            {
                new ActivityLog { UserCnp = "1234567890123", ActivityName = "Deposit", LastModifiedAmount = 5000, ActivityDetails = "Deposited funds into savings account", CreatedAt = new DateTime(2025, 4, 1) },
                new ActivityLog { UserCnp = "9876543210987", ActivityName = "Withdrawal", LastModifiedAmount = -1200, ActivityDetails = "Withdrew cash from ATM", CreatedAt = new DateTime(2025, 3, 15) },
                new ActivityLog { UserCnp = "2345678901234", ActivityName = "Investment", LastModifiedAmount = 3500, ActivityDetails = "Invested in stock market", CreatedAt = new DateTime(2025, 2, 20) },
                new ActivityLog { UserCnp = "3456789012345", ActivityName = "Loan Payment", LastModifiedAmount = -800, ActivityDetails = "Paid monthly loan installment", CreatedAt = new DateTime(2025, 1, 10) },
                new ActivityLog { UserCnp = "4567890123456", ActivityName = "Account Transfer", LastModifiedAmount = -1500, ActivityDetails = "Transferred funds to another account", CreatedAt = new DateTime(2025, 5, 5) }
            };

            await context.ActivityLogs.AddRangeAsync(activityLogs);
        }
    }
}