using Common.Models.Bank;

namespace BankApi.Repositories.Impl.Bank
{
    public interface IBankAccountRepository
    {
        Task<BankAccount> CreateBankAccountAsync(BankAccount bankAccount);
        Task<bool> DeleteBankAccountAsync(string iban);
        Task<List<BankAccount>> GetAllBankAccountsAsync();
        Task<BankAccount> GetBankAccountBalanceByIbanAsync(string IBAN);
        Task<List<BankAccount>> GetBankAccountsByUserIdAsync(int userId);
        Task<bool> UpdateBankAccountAsync(BankAccount bankAccount);
    }
}