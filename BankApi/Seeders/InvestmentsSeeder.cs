using BankApi.Data;
using Common.Models;

namespace BankApi.Seeders
{
    public class InvestmentsSeeder(IConfiguration configuration, IServiceProvider serviceProvider) : RegularTableSeeder<Investment>(configuration, serviceProvider)
    {
        protected override async Task SeedDataAsync(ApiDbContext context)
        {
            if (context.Investments.Any())
            {
                Console.WriteLine("Investments already exist, skipping seeding.");
                return;
            }

            var investments = new[]
            {
                new Investment { InvestorCnp = "1234567890123", Details = "Tech startup funding", AmountInvested = 5000.00m, AmountReturned = 5500.00m, InvestmentDate = new DateTime(2025, 4, 1) },
                new Investment { InvestorCnp = "9876543210987", Details = "Stock market investment", AmountInvested = 12000.50m, AmountReturned = 13000.00m, InvestmentDate = new DateTime(2025, 3, 15) },
                new Investment { InvestorCnp = "2345678901234", Details = "Real estate project", AmountInvested = 3500.75m, AmountReturned = 4000.00m, InvestmentDate = new DateTime(2025, 2, 20) },
                new Investment { InvestorCnp = "3456789012345", Details = "Cryptocurrency investment", AmountInvested = 8000.00m, AmountReturned = 7500.00m, InvestmentDate = new DateTime(2025, 1, 10) },
                new Investment { InvestorCnp = "4567890123456", Details = "Renewable energy initiative", AmountInvested = 15000.25m, AmountReturned = 16000.00m, InvestmentDate = new DateTime(2025, 5, 5) }
            };

            await context.Investments.AddRangeAsync(investments);
        }
    }
}