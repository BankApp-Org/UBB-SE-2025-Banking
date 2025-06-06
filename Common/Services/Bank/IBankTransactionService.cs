using Common.DTOs;
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

        Task<List<TransactionTypeCountDTO>> GetTransactionTypeCounts(int userId);
    }
    public class TransactionTypeCountDTO
    {
        required public TransactionTypeDTO TransactionType { get; set; }
        required public int Count { get; set; }
    }
    public class TransactionFilters
    {
        public TransactionType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? SenderIban { get; set; }
        public string? ReceiverIban { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public Ordering Ordering { get; set; } = Ordering.Ascending;
    }
}
