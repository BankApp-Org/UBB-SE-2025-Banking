using BankApi.Data;
using Common.Models.Bank;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Seeders
{
    public class BankSeeders : TableSeeder
    {
        public BankSeeders(IConfiguration configuration, IServiceProvider serviceProvider)
            : base(configuration, serviceProvider)
        {
        }

        public override async Task SeedAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

            if (await dbContext.BankAccounts.AnyAsync())
            {
                Console.WriteLine("Bank accounts already seeded.");
                return;
            }

            var users = await dbContext.Users.ToListAsync();

            var accounts = new List<BankAccount>();
            int ibanCounter = 10000001;

            foreach (var user in users)
            {
                // Main Account (in RON or EUR)
                accounts.Add(new BankAccount
                {
                    Iban = $"RO42BANKRON0{ibanCounter++}",
                    Currency = Currency.RON,
                    Balance = user.Balance,
                    Blocked = false,
                    DailyLimit = 3000m,
                    MaximumPerTransaction = 1500m,
                    MaximumNrTransactions = 15,
                    Name = "Main Account",
                    UserId = user.Id,
                    User = user,
                    Transactions = []
                });

                // Secondary/Savings Account
                accounts.Add(new BankAccount
                {
                    Iban = $"RO42BANKEUR0{ibanCounter++}",
                    Currency = Currency.EUR,
                    Balance = user.Balance / 2, // Savings typically lower
                    Blocked = false,
                    DailyLimit = 1000m,
                    MaximumPerTransaction = 500m,
                    MaximumNrTransactions = 5,
                    Name = "Savings Account",
                    UserId = user.Id,
                    User = user,
                    Transactions = []
                });

                Console.WriteLine($"Assigned 2 accounts for {user.UserName}");
            }

            dbContext.BankAccounts.AddRange(accounts);
            await dbContext.SaveChangesAsync();
            Console.WriteLine($"Seeded {accounts.Count} bank accounts.");
        }





    }
}
