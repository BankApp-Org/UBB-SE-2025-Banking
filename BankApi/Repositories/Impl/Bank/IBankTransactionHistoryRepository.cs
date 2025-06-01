using Common.Models.Bank;

namespace BankApi.Repositories.Impl.Bank
{
    public interface IBankTransactionHistoryRepository
    {
        Task CreateTransactionAsync(BankTransaction transaction);
        Task DeleteTransactionAsync(int transactionId);
        Task<List<BankTransaction>> GetAllTransactionsAsync();
        Task<BankTransaction> GetTransactionByIdAsync(int transactionId);
        Task<List<BankTransaction>> GetTransactionHistoryByIbanAsync(string iban);
        Task UpdateTransactionAsync(BankTransaction transaction);
    }
}