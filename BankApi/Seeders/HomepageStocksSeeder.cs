using BankApi.Data;
using Common.Models; // Assuming HomepageStock model is in Common.Models
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
    public class HomepageStocksSeeder(IConfiguration configuration, IServiceProvider serviceProvider) : ForeignKeyTableSeeder(configuration, serviceProvider)
    {

        // Implemented the abstract method SeedDataAsync
        protected override async Task SeedDataAsync(ApiDbContext context)
        {
            if (await context.HomepageStocks.AnyAsync())
            {
                Console.WriteLine("HomepageStocks already exist, skipping seeding.");
                return;
            }

            // These symbols should correspond to Stock entities created by BaseStocksSeeder
            var stockSymbolsToLink = new[] { "AAPL", "GOOGL", "TSLA", "AMZN", "MSFT" };
            var existingStocks = await context.Stocks
                                          .Where(s => stockSymbolsToLink.Contains(s.Symbol))
                                          .ToListAsync();

            if (existingStocks.Count < stockSymbolsToLink.Length)
            {
                Console.WriteLine("Warning: Not all specified stocks for HomepageStocksSeeder were found. Seeding might be incomplete.");
            }

            var homepageStocksToSeed = new List<HomepageStock>();

            var stockData = new[]
            {
                new { Symbol = "AAPL", Change = 1.25m },
                new { Symbol = "GOOGL", Change = -0.75m },
                new { Symbol = "TSLA", Change = 2.30m },
                new { Symbol = "AMZN", Change = -1.10m },
                new { Symbol = "MSFT", Change = 0.50m }
            };

            foreach (var data in stockData)
            {
                var relatedStock = existingStocks.FirstOrDefault(s => s.Symbol == data.Symbol);
                if (relatedStock != null)
                {
                    // The Id for HomepageStock should match the Id of the related Stock entity
                    // as per the one-to-one relationship defined in ApiDbContext.OnModelCreating
                    homepageStocksToSeed.Add(new HomepageStock
                    {
                        Id = relatedStock.Id, // This is crucial for the one-to-one relationship
                        Symbol = relatedStock.Symbol,
                        Change = data.Change,
                        StockDetails = relatedStock // Link the actual stock entity
                    });
                }
                else
                {
                    Console.WriteLine($"Skipping HomepageStock for Symbol: {data.Symbol} as related stock does not exist.");
                }
            }

            if (homepageStocksToSeed.Count != 0)
            {
                await context.HomepageStocks.AddRangeAsync(homepageStocksToSeed);
            }
            else
            {
                Console.WriteLine("No valid homepage stocks to seed due to missing related stock entities.");
            }
        }
    }
}