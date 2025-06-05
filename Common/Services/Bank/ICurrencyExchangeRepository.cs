using Common.Models.Bank;

namespace Common.Services.Bank
{
    public interface ICurrencyExchangeRepository
    {
        Task AddExchangeRateAsync(CurrencyExchange exchangeRate);
        Task DeleteExchangeRateAsync(int exchangeRateId);
        Task<List<CurrencyExchange>> GetAllExchangeRatesAsync();
        Task<CurrencyExchange> GetExchangeRateAsync(Currency fromCurrency, Currency toCurrency);
        Task UpdateExchangeRateAsync(CurrencyExchange exchangeRate);
    }
}