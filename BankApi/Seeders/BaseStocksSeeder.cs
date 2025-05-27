using BankApi.Data;
using Common.Models; // Assuming BaseStock and Stock models are in Common.Models

namespace BankApi.Seeders
{
    public class BaseStocksSeeder(IConfiguration configuration, IServiceProvider serviceProvider) : RegularTableSeeder<BaseStock>(configuration, serviceProvider)
    {
        protected override async Task SeedDataAsync(ApiDbContext context)
        {
            if (context.Stocks.Any()) // Check Stocks table as it represents the concrete type
            {
                Console.WriteLine("Stocks (BaseStocks) already exist, skipping seeding.");
                return;
            }

            var stocksToSeed = new[]
            {
                    new Stock
                    {
                        Name = "Apple Inc.",
                        Symbol = "AAPL",
                        AuthorCNP = "1234567890123",
                        Price = 175,
                        Quantity = 100,
                        NewsArticles = []
                    },
                    new Stock
                    {
                        Name = "Alphabet Inc.",
                        Symbol = "GOOGL",
                        AuthorCNP = "9876543210987",
                        Price = 2850,
                        Quantity = 50,
                        NewsArticles = []
                    },
                    new Stock
                    {
                        Name = "Tesla Inc.",
                        Symbol = "TSLA",
                        AuthorCNP = "2345678901234",
                        Price = 725,
                        Quantity = 200,
                        NewsArticles = []
                    },
                    new Stock
                    {
                        Name = "Amazon.com Inc.",
                        Symbol = "AMZN",
                        AuthorCNP = "3456789012345",
                        Price = 3425,
                        Quantity = 80,
                        NewsArticles = []
                    },
                    new Stock
                    {
                        Name = "Microsoft Corp.",
                        Symbol = "MSFT",
                        AuthorCNP = "4567890123456",
                        Price = 325,
                        Quantity = 150,
                        NewsArticles = []
                    }
                };

            await context.Stocks.AddRangeAsync(stocksToSeed);
        }
    }
}