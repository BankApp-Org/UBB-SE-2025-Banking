using BankApi.Data;
using Common.Models.Trading; // Assuming Alert model is in Common.Models

namespace BankApi.Seeders
{
    public class AlertsSeeder(IConfiguration configuration, IServiceProvider serviceProvider) : RegularTableSeeder<Alert>(configuration, serviceProvider)
    {

        // Implemented the abstract method SeedDataAsync
        protected override async Task SeedDataAsync(ApiDbContext context)
        {
            if (context.Alerts.Any())
            {
                Console.WriteLine("Alerts already exist, skipping seeding.");
                return;
            }

            var alerts = new[]
            {
                new Alert { StockName = "AAPL", Name = "Apple Stock Alert", UpperBound = 180.00m, LowerBound = 120.00m, ToggleOnOff = true },
                new Alert { StockName = "GOOGL", Name = "Alphabet Price Watch", UpperBound = 2900.00m, LowerBound = 2500.00m, ToggleOnOff = true },
                new Alert { StockName = "TSLA", Name = "Tesla Trading Alert", UpperBound = 800.00m, LowerBound = 600.00m, ToggleOnOff = true },
                new Alert { StockName = "AMZN", Name = "Amazon Stock Monitor", UpperBound = 3500.00m, LowerBound = 3000.00m, ToggleOnOff = false },
                new Alert { StockName = "MSFT", Name = "Microsoft Price Alert", UpperBound = 350.00m, LowerBound = 280.00m, ToggleOnOff = true }
            };

            await context.Alerts.AddRangeAsync(alerts);
        }
    }
}