using BankApi.Data;
using Common.Models; // Assuming TriggeredAlert model is in Common.Models
using Microsoft.EntityFrameworkCore; // Required for AnyAsync

namespace BankApi.Seeders
{
    public class TriggeredAlertsSeeder(IConfiguration configuration, IServiceProvider serviceProvider) : RegularTableSeeder<TriggeredAlert>(configuration, serviceProvider)
    {

        // Implemented the abstract method SeedDataAsync
        protected override async Task SeedDataAsync(ApiDbContext context)
        {
            if (await context.TriggeredAlerts.AnyAsync())
            {
                Console.WriteLine("TriggeredAlerts already exist, skipping seeding.");
                return;
            }

            var triggeredAlerts = new[]
            {
                new TriggeredAlert { StockName = "AAPL", Message = "Stock price exceeded upper bound of $180.00", TriggeredAt = new DateTime(2025, 4, 1) },
                new TriggeredAlert { StockName = "GOOGL", Message = "Stock price dropped below lower bound of $2500.00", TriggeredAt = new DateTime(2025, 3, 15) },
                new TriggeredAlert { StockName = "TSLA", Message = "Rapid volume surge detected", TriggeredAt = new DateTime(2025, 2, 20) },
                new TriggeredAlert { StockName = "AMZN", Message = "Stock price exceeded upper bound of $3500.00", TriggeredAt = new DateTime(2025, 1, 10) },
                new TriggeredAlert { StockName = "MSFT", Message = "Unusual market movement in after-hours trading", TriggeredAt = new DateTime(2025, 5, 5) }
            };

            // It's good practice to ensure the StockName in TriggeredAlerts refers to existing stocks.
            // This seeder should ideally run after BaseStocksSeeder.
            var validTriggeredAlerts = new List<TriggeredAlert>();
            foreach (var ta in triggeredAlerts)
            {
                var stockExists = await context.Stocks.AnyAsync(s => s.Name == ta.StockName);
                if (stockExists) // Or s.Symbol if StockName in TriggeredAlert refers to Symbol
                {
                    validTriggeredAlerts.Add(ta);
                }
                else
                {
                    Console.WriteLine($"Skipping TriggeredAlert for StockName: {ta.StockName} as related stock does not exist.");
                }
            }

            if (validTriggeredAlerts.Count != 0)
            {
                await context.TriggeredAlerts.AddRangeAsync(validTriggeredAlerts);
            }
        }
    }
}