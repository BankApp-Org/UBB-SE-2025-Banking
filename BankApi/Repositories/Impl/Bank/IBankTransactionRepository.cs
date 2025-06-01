using Common.Models.Bank;

namespace BankApi.Repositories.Impl.Bank
{
    public interface IBankTransactionRepository
    {
        Task<bool> AddTransaction(BankTransaction transaction);
        Task<List<BankAccount>> GetAllBankAccounts();
        Task<List<CurrencyExchange>> GetAllCurrencyExchangeRates();
        Task<BankAccount> GetBankAccountByIBAN(string iban);
        Task<List<BankTransaction>> GetBankAccountTransactions(string iban);
        Task<decimal> GetExchangeRate(Currency fromCurrency, Currency toCurrency);
        Task<bool> UpdateBankAccountBalance(string iban, decimal newBalance);
    }
}