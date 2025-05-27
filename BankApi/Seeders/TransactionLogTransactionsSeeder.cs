using BankApi.Data;
using Common.Models; // Assuming TransactionLogTransaction model is in Common.Models
using Microsoft.EntityFrameworkCore; // Required for AnyAsync and other EF Core methods

namespace BankApi.Seeders
{
    // Updated to inherit from ForeignKeyTableSeeder (which is not generic anymore)
    public class TransactionLogTransactionsSeeder(IConfiguration configuration, IServiceProvider serviceProvider) : ForeignKeyTableSeeder(configuration, serviceProvider)
    {

        // Implemented the abstract method SeedDataAsync
        protected override async Task SeedDataAsync(ApiDbContext context)
        {
            if (await context.TransactionLogTransactions.AnyAsync())
            {
                Console.WriteLine("TransactionLogTransactions already exist, skipping seeding.");
                return;
            }

            // Sample CNPs of users that should already exist from UsersSeeder
            var userCnps = new[] { "1234567890123", "9876543210987", "2345678901234", "3456789012345", "4567890123456" };

            var transactionsToSeed = new List<TransactionLogTransaction>();
            var users = await context.Users.Where(u => userCnps.Contains(u.CNP)).ToListAsync();

            if (users.Count < userCnps.Length)
            {
                Console.WriteLine("Warning: Not all specified users for TransactionLogTransactionsSeeder were found. Seeding might be incomplete or fail if relationships are strict.");
            }

            // Ensure Stocks also exist (from BaseStocksSeeder)
            var stockNames = new[] { "Apple Inc.", "Alphabet Inc.", "Tesla Inc.", "Amazon.com Inc.", "Microsoft Corp." };
            var stocks = await context.Stocks.Where(s => stockNames.Contains(s.Name)).ToListAsync();

            if (stocks.Count < stockNames.Length)
            {
                Console.WriteLine("Warning: Not all specified stocks for TransactionLogTransactionsSeeder were found. Seeding might be incomplete or fail.");
            }

            // Create transactions only if the corresponding user and stock exist
            var user1 = users.FirstOrDefault(u => u.CNP == "1234567890123");
            var stockApple = stocks.FirstOrDefault(s => s.Name == "Apple Inc.");
            var user2 = users.FirstOrDefault(u => u.CNP == "9876543210987");
            var stockGoogle = stocks.FirstOrDefault(s => s.Name == "Alphabet Inc.");
            var user3 = users.FirstOrDefault(u => u.CNP == "2345678901234");
            var stockTesla = stocks.FirstOrDefault(s => s.Name == "Tesla Inc.");
            var user4 = users.FirstOrDefault(u => u.CNP == "3456789012345");
            var stockAmazon = stocks.FirstOrDefault(s => s.Name == "Amazon.com Inc.");
            var user5 = users.FirstOrDefault(u => u.CNP == "4567890123456");
            var stockMicrosoft = stocks.FirstOrDefault(s => s.Name == "Microsoft Corp.");
            if (user1 != null && stockApple != null)
            {
                transactionsToSeed.Add(new TransactionLogTransaction
                {
                    AuthorCNP = user1.CNP,
                    Author = user1,
                    StockSymbol = stockApple.Symbol,
                    StockName = stockApple.Name,
                    Type = "BUY",
                    Amount = 10,
                    PricePerStock = 150,
                    Date = new DateTime(2025, 4, 1)
                });
            }

            if (user2 != null && stockGoogle != null)
            {
                transactionsToSeed.Add(new TransactionLogTransaction
                {
                    AuthorCNP = user2.CNP,
                    Author = user2,
                    StockSymbol = stockGoogle.Symbol,
                    StockName = stockGoogle.Name,
                    Type = "SELL",
                    Amount = 5,
                    PricePerStock = 2800,
                    Date = new DateTime(2025, 3, 15)
                });
            }

            if (user3 != null && stockTesla != null)
            {
                transactionsToSeed.Add(new TransactionLogTransaction
                {
                    AuthorCNP = user3.CNP,
                    Author = user3,
                    StockSymbol = stockTesla.Symbol,
                    StockName = stockTesla.Name,
                    Type = "BUY",
                    Amount = 20,
                    PricePerStock = 750,
                    Date = new DateTime(2025, 2, 20)
                });
            }

            if (user4 != null && stockAmazon != null)
            {
                transactionsToSeed.Add(new TransactionLogTransaction
                {
                    AuthorCNP = user4.CNP,
                    Author = user4,
                    StockSymbol = stockAmazon.Symbol,
                    StockName = stockAmazon.Name,
                    Type = "SELL",
                    Amount = 8,
                    PricePerStock = 3400,
                    Date = new DateTime(2025, 1, 10)
                });
            }

            if (user5 != null && stockMicrosoft != null)
            {
                transactionsToSeed.Add(new TransactionLogTransaction
                {
                    AuthorCNP = user5.CNP,
                    Author = user5,
                    StockSymbol = stockMicrosoft.Symbol,
                    StockName = stockMicrosoft.Name,
                    Type = "BUY",
                    Amount = 15,
                    PricePerStock = 320,
                    Date = new DateTime(2025, 5, 5)
                });
            }

            if (transactionsToSeed.Count != 0)
            {
                await context.TransactionLogTransactions.AddRangeAsync(transactionsToSeed);
            }
            else
            {
                Console.WriteLine("No valid transaction log entries to seed due to missing related users or stocks.");
            }
        }
    }
}