namespace Common.Services
{
    using Common.Models.Trading;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ITransactionLogService
    {
        Task<List<StockTransaction>>
            GetFilteredTransactions(StockTransactionFilterCriteria criteria);

        List<StockTransaction>
            SortTransactions(
                List<StockTransaction> transactions,
                string sortType = "Date",
                bool ascending = true);

        void ExportTransactions(
            List<StockTransaction> transactions,
            string filePath,
            string format);
    }
}
