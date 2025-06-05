using Common.Models.Trading;
using Common.Services.Trading;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class StockTransactionLogProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : ITransactionLogService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task<List<StockTransaction>> GetFilteredTransactions(StockTransactionFilterCriteria criteria)
        {
            var response = await _httpClient.PostAsJsonAsync("api/TransactionLog/filter", criteria);
            response.EnsureSuccessStatusCode();
            var transactions = await response.Content.ReadFromJsonAsync<IEnumerable<StockTransaction>>(_jsonOptions) ?? [];
            return [.. transactions];
        }

        public List<StockTransaction> SortTransactions(List<StockTransaction> transactions, string sortType = "Date", bool ascending = true)
        {
            return sortType.ToLower() switch
            {
                "stockname" => ascending
                    ? [.. transactions.OrderBy(t => t.StockName)]
                    : [.. transactions.OrderByDescending(t => t.StockName)],
                "type" => ascending
                    ? [.. transactions.OrderBy(t => t.Type)]
                    : [.. transactions.OrderByDescending(t => t.Type)],
                "amount" => ascending
                    ? [.. transactions.OrderBy(t => t.Amount)]
                    : [.. transactions.OrderByDescending(t => t.Amount)],
                "totalvalue" => ascending
                    ? [.. transactions.OrderBy(t => t.TotalValue)]
                    : [.. transactions.OrderByDescending(t => t.TotalValue)],
                "date" => ascending
                    ? [.. transactions.OrderBy(t => t.Date)]
                    : [.. transactions.OrderByDescending(t => t.Date)],
                "author" => ascending
                    ? [.. transactions.OrderBy(t => t.Author)]
                    : [.. transactions.OrderByDescending(t => t.Author)],
                _ => transactions
            };
        }

        public void ExportTransactions(List<StockTransaction> transactions, string filePath, string format)
        {
            var url = $"api/transactionlog/export/{format}?filePath={filePath}";
            var response = _httpClient.PostAsJsonAsync(url, transactions, _jsonOptions).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }
    }
}