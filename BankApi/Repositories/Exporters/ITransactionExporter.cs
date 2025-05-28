namespace BankApi.Repositories.Exporters
{
    using Common.Models.Trading;
    using System.Collections.Generic;

    public interface ITransactionExporter
    {
        void Export(List<StockTransaction> transactions, string filePath);
    }
}
