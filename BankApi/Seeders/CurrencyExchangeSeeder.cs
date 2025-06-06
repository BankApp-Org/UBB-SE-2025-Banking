using BankApi.Data;
using Common.Models.Bank;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Seeders
{
    public class CurrencyExchangeSeeder : TableSeeder
    {
        public CurrencyExchangeSeeder(IConfiguration configuration, IServiceProvider serviceProvider)
            : base(configuration, serviceProvider)
        {
        }

        public override async Task SeedAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

            if (await dbContext.CurrencyExchanges.AnyAsync())
            {
                Console.WriteLine("Currency exchanges already seeded.");
                return;
            }

            var exchangeRates = new List<CurrencyExchange>
            {
                new() { FromCurrency = Currency.USD, ToCurrency = Currency.EUR, ExchangeRate = 0.8752m },
                new() { FromCurrency = Currency.USD, ToCurrency = Currency.GBP, ExchangeRate = 0.7383m },
                new() { FromCurrency = Currency.USD, ToCurrency = Currency.JPY, ExchangeRate = 144.01m },
                new() { FromCurrency = Currency.USD, ToCurrency = Currency.RON, ExchangeRate = 4.39m },
                new() { FromCurrency = Currency.EUR, ToCurrency = Currency.USD, ExchangeRate = 1.1423m },
                new() { FromCurrency = Currency.EUR, ToCurrency = Currency.GBP, ExchangeRate = 0.8419m },
                new() { FromCurrency = Currency.EUR, ToCurrency = Currency.JPY, ExchangeRate = 163.66m },
                new() { FromCurrency = Currency.EUR, ToCurrency = Currency.RON, ExchangeRate = 5.0505m },
                new() { FromCurrency = Currency.GBP, ToCurrency = Currency.USD, ExchangeRate = 1.3539m },
                new() { FromCurrency = Currency.GBP, ToCurrency = Currency.EUR, ExchangeRate = 1.1849m },
                new() { FromCurrency = Currency.GBP, ToCurrency = Currency.JPY, ExchangeRate = 194.98m },
                new() { FromCurrency = Currency.GBP, ToCurrency = Currency.RON, ExchangeRate = 5.99m },
                new() { FromCurrency = Currency.JPY, ToCurrency = Currency.USD, ExchangeRate = 0.0069m },
                new() { FromCurrency = Currency.JPY, ToCurrency = Currency.EUR, ExchangeRate = 0.0061m },
                new() { FromCurrency = Currency.JPY, ToCurrency = Currency.GBP, ExchangeRate = 0.0051m },
                new() { FromCurrency = Currency.JPY, ToCurrency = Currency.RON, ExchangeRate = 0.0307m },
                new() { FromCurrency = Currency.RON, ToCurrency = Currency.USD, ExchangeRate = 0.2278m },
                new() { FromCurrency = Currency.RON, ToCurrency = Currency.EUR, ExchangeRate = 0.2006m },
                new() { FromCurrency = Currency.RON, ToCurrency = Currency.GBP, ExchangeRate = 0.1670m },
                new() { FromCurrency = Currency.RON, ToCurrency = Currency.JPY, ExchangeRate = 32.57m },
            };

            dbContext.CurrencyExchanges.AddRange(exchangeRates);
            await dbContext.SaveChangesAsync();

            Console.WriteLine("Seeded currency exchange rates.");
        }

    }
}
