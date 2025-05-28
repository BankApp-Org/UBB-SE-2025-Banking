using BankApi.Data;
using Common.Models.Trading;
using Microsoft.EntityFrameworkCore; // Required for AnyAsync

namespace BankApi.Seeders
{
    public class UserStocksSeeder(IConfiguration configuration, IServiceProvider serviceProvider) : RegularTableSeeder<UserStock>(configuration, serviceProvider)
    {
        protected override async Task SeedDataAsync(ApiDbContext context)
        {
            if (await context.UserStocks.AnyAsync()) // Added await and ensured AnyAsync is available
            {
                Console.WriteLine("UserStocks already exist, skipping seeding.");
                return;
            }

            var userStocksData = new[]
            {
                new { UserCnp = "1234567890123", StockName = "Apple Inc.", Quantity = 10 },
                new { UserCnp = "9876543210987", StockName = "Alphabet Inc.", Quantity = 5 },
                new { UserCnp = "2345678901234", StockName = "Tesla Inc.", Quantity = 20 },
                new { UserCnp = "3456789012345", StockName = "Amazon.com Inc.", Quantity = 8 },
                new { UserCnp = "4567890123456", StockName = "Microsoft Corp.", Quantity = 15 }
            };

            var validUserStocks = new List<UserStock>();
            foreach (var usData in userStocksData)
            {
                // Fetch the actual User and Stock entities
                var user = await context.Users.FirstOrDefaultAsync(u => u.CNP == usData.UserCnp);
                var stock = await context.Stocks.FirstOrDefaultAsync(s => s.Name == usData.StockName);

                if (user != null && stock != null)
                {
                    validUserStocks.Add(new UserStock
                    {
                        UserCnp = usData.UserCnp,
                        StockName = usData.StockName,
                        Quantity = usData.Quantity,
                        User = user,
                        Stock = stock
                    });
                }
                else
                {
                    Console.WriteLine($"Skipping UserStock for UserCNP: {usData.UserCnp}, StockName: {usData.StockName} as related entity does not exist.");
                }

            }

            if (validUserStocks.Count != 0)
            {
                await context.UserStocks.AddRangeAsync(validUserStocks);
            }
        }
    }
}