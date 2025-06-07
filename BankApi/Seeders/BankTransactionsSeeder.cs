using BankApi.Data;
using Common.Models.Bank;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Seeders
{
    public class BankTransactionsSeeder : TableSeeder
    {
        public BankTransactionsSeeder(IConfiguration configuration, IServiceProvider serviceProvider)
            : base(configuration, serviceProvider)
        {}

        public override async Task SeedAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

            if (await dbContext.BankTransactions.AnyAsync())
            {
                Console.WriteLine("Bank accounts already seeded.");
                return;
            }

            var bankAccounts = await dbContext.BankAccounts.ToListAsync();
            var transactions = new List<BankTransaction>();

            Random random = new Random();

            foreach (var bankAccount1 in bankAccounts)
            {
                for(int i = 0; i < Math.Min(2, bankAccounts.Count); i++) {
                    var bankAccount2 = bankAccounts[i];
                    if (bankAccount1.Iban == bankAccount2.Iban)
                        continue;

                    var ammount = random.Next(10, 400);
                    TransactionType type = (TransactionType) random.Next(0, 10); 

                    var transaction = new BankTransaction
                    {
                        SenderIban = bankAccount1.Iban,
                        ReceiverIban = bankAccount2.Iban,
                        SenderAmount = ammount,
                        ReceiverAmount = ammount,
                        SenderCurrency = bankAccount1.Currency,
                        ReceiverCurrency = bankAccount2.Currency,
                        TransactionDescription = "banii de cantina",
                        TransactionType = type,
                        TransactionDatetime = DateTime.Now,
                    };

                    transactions.Add(transaction);
                }
            }

            dbContext.BankTransactions.AddRange(transactions);
            await dbContext.SaveChangesAsync();
            Console.WriteLine($"Seeded {transactions.Count} bank accounts.");
        }
    }
}
