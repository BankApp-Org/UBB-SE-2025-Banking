using System.Collections.Generic;
using System.Threading.Tasks;
using BankAppDesktop.DTO;

namespace BankAppDesktop.Services
{
    public interface ITransactionHistoryService
    {
        Task<List<TransactionTypeCountDTO>> GetTransactionTypeCounts(string userId);
    }

    public class TransactionTypeCountDTO
    {
        public TransactionTypeDTO TransactionType { get; set; }
        public int Count { get; set; }
    }
}