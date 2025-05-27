namespace BankApi.Repositories.Exporters
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Common.Models;

    public class CSVTransactionExporter : ITransactionExporter
    {
        public void Export(List<TransactionLogTransaction> transactions, string filePath)
        {
            using StreamWriter writer = new(filePath);
            writer.WriteLine("StockSymbol,StockName,TransactionType,Amount,PricePerStock,TotalValue,Date,Author");

            foreach (var transaction in transactions)
            {
                var csvRow =
                    $"{transaction.StockSymbol}," +
                    $"{transaction.StockName}," +
                    $"{transaction.Type}," +
                    $"{transaction.Amount}," +
                    $"{transaction.PricePerStock}," +
                    $"{transaction.TotalValue}," +
                    $"{transaction.Date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}," +
                    $"{transaction.Author}";

                writer.WriteLine(csvRow);
            }
        }
    }
}
