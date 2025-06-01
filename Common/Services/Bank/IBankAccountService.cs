using Common.Models.Bank;

namespace Common.Services.Bank
{
    public interface IBankAccountService
    {
        Task<List<BankAccount>> GetUserBankAccounts(int userID);
        Task<BankAccount?> FindBankAccount(string iban);
        Task<bool> CreateBankAccount(BankAccount bankAccount);
        Task<bool> RemoveBankAccount(string iban);
        Task<bool> CheckIBANExists(string iban);
        Task<string> GenerateIBAN();
        Task<bool> UpdateBankAccount(BankAccount bankAccount);
        Task<decimal> ConvertCurrency(decimal amount, Currency fromCurrency, Currency toCurrency);
        Task<CurrencyExchange> GetExchangeRateAsync(Currency fromCurrency, Currency toCurrency);
        Task<bool> CheckSufficientFunds(int userID, string accountIBAN, decimal amount, Currency currency);
    }
}
