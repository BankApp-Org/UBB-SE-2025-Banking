using Common.Models.Trading;
using Common.Services.Trading;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class TransactionProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : ITransactionService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task<StockTransaction> CreateTransactionAsync(StockTransaction transaction)
        {
            var response = await _httpClient.PostAsJsonAsync("api/transaction", transaction, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<StockTransaction>(_jsonOptions) ?? throw new InvalidOperationException("Failed to deserialize response");
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/transaction/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<StockTransaction>> GetAllTransactionsAsync()
        {
            var transactions = await _httpClient.GetFromJsonAsync<IEnumerable<StockTransaction>>("api/transaction", _jsonOptions) ?? [];
            return [.. transactions];
        }

        public async Task<StockTransaction> GetTransactionByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<StockTransaction>($"api/transaction/{id}", _jsonOptions) ?? throw new InvalidOperationException("Transaction not found");
        }

        public async Task<StockTransaction> UpdateTransactionAsync(int id, StockTransaction transaction)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/transaction/{id}", transaction, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<StockTransaction>(_jsonOptions) ?? throw new InvalidOperationException("Failed to deserialize response");
        }

        public async Task<List<StockTransaction>> GetUserTransactionsAsync(string userId)
        {
            var transactions = await _httpClient.GetFromJsonAsync<IEnumerable<StockTransaction>>($"api/transaction/user/{userId}", _jsonOptions) ?? [];
            return [.. transactions];
        }

        public async Task<List<StockTransaction>> GetFilteredAndSortedTransactionsAsync(string searchTerm, string sortBy, bool ascending, string? userId = null)
        {
            var url = $"api/transaction/filter?searchTerm={searchTerm}&sortBy={sortBy}&ascending={ascending}";
            if (userId != null)
            {
                url += $"&userId={userId}";
            }
            var transactions = await _httpClient.GetFromJsonAsync<IEnumerable<StockTransaction>>(url, _jsonOptions) ?? [];
            return [.. transactions];
        }

        public async Task<List<StockTransaction>> GetByFilterCriteriaAsync(StockTransactionFilterCriteria criteria)
        {
            var response = await _httpClient.PostAsJsonAsync("api/transaction/filter", criteria, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var transactions = await response.Content.ReadFromJsonAsync<IEnumerable<StockTransaction>>(_jsonOptions) ?? [];
            return [.. transactions];
        }

        public async Task AddTransactionAsync(StockTransaction transaction)
        {
            var response = await _httpClient.PostAsJsonAsync("api/transaction", transaction, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }
    }
}