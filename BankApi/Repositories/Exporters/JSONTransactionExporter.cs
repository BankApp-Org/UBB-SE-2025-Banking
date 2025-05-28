namespace BankApi.Repositories.Exporters
{
    using Common.Models.Trading;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;

    public class JSONTransactionExporter : ITransactionExporter
    {
        private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

        public void Export(List<StockTransaction> transactions, string filePath)
        {
            var jsonData = JsonSerializer.Serialize(transactions, Options);
            File.WriteAllText(filePath, jsonData);
        }
    }
}
