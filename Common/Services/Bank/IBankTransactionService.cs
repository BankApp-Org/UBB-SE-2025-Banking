using Common.Models;
using Common.Models.Bank;

namespace Common.Services.Bank
{
    public interface IBankTransactionService
    {
        Task<List<BankTransaction>> GetTransactions(TransactionFilters transactionFilters);

        Task<BankTransaction?> GetTransactionById(int transactionId);

        Task<bool> CreateTransaction(BankTransaction transaction);

        Task<bool> UpdateTransaction(BankTransaction transaction);

        Task<bool> DeleteTransaction(int transactionId);

    }
    public class TransactionFilters
    {
        required public TransactionType Type { get; set; }
        required public DateTime StartDate { get; set; }
        required public DateTime EndDate { get; set; }
        required public string? SenderIban { get; set; }
        required public string? ReceiverIban { get; set; }
        required public decimal? MinAmount { get; set; }
        required public decimal? MaxAmount { get; set; }
        required public Ordering Ordering { get; set; } = Ordering.Ascending;
    }
}
