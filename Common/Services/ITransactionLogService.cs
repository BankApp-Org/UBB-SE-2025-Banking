namespace Common.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Models;

    public interface ITransactionLogService
    {
        Task<List<TransactionLogTransaction>>
            GetFilteredTransactions(TransactionFilterCriteria criteria);

        List<TransactionLogTransaction>
            SortTransactions(
                List<TransactionLogTransaction> transactions,
                string sortType = "Date",
                bool ascending = true);

        void ExportTransactions(
            List<TransactionLogTransaction> transactions,
            string filePath,
            string format);
    }
}
