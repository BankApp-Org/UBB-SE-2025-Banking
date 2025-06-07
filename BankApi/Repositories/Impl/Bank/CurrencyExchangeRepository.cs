using BankApi.Data;
using Common.Models.Bank;
using Common.Services.Bank;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Repositories.Impl.Bank
{
    public class CurrencyExchangeRepository : ICurrencyExchangeRepository
    {
        private readonly ApiDbContext _context;
        public CurrencyExchangeRepository(ApiDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<CurrencyExchange> GetExchangeRateAsync(Currency fromCurrency, Currency toCurrency)
        {
            return await _context.CurrencyExchanges
                .FirstOrDefaultAsync(ce => ce.FromCurrency == fromCurrency && ce.ToCurrency == toCurrency)
                ?? throw new Exception($"Exchange rate from {fromCurrency} to {toCurrency} not found.");
        }
        public async Task<List<CurrencyExchange>> GetAllExchangeRatesAsync()
        {
            return await _context.CurrencyExchanges.ToListAsync();
        }

        public async Task AddExchangeRateAsync(CurrencyExchange exchangeRate)
        {
            if (exchangeRate == null)
            {
                throw new ArgumentNullException(nameof(exchangeRate), "Exchange rate cannot be null.");
            }
            _context.CurrencyExchanges.Add(exchangeRate);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateExchangeRateAsync(CurrencyExchange exchangeRate)
        {
            if (exchangeRate == null)
            {
                throw new ArgumentNullException(nameof(exchangeRate), "Exchange rate cannot be null.");
            }
            _context.CurrencyExchanges.Update(exchangeRate);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteExchangeRateAsync(int exchangeRateId)
        {
            var exchangeRate = await _context.CurrencyExchanges.FindAsync(exchangeRateId);
            if (exchangeRate == null)
            {
                throw new KeyNotFoundException($"Exchange rate with ID {exchangeRateId} not found.");
            }
            _context.CurrencyExchanges.Remove(exchangeRate);
            await _context.SaveChangesAsync();
        }
    }
}
