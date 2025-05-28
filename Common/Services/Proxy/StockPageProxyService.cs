using Common.Models.Trading;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class StockPageProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IProxyService, IStockPageService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task<bool> BuyStockAsync(string stockName, int quantity, string? userCNP = null)
        {
            var response = await _httpClient.PostAsJsonAsync("api/StockPage/buy", new { stockName, quantity }, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
        }

        public async Task<bool> GetFavoriteAsync(string stockName, string? userCNP = null)
        {
            return await _httpClient.GetFromJsonAsync<bool>($"api/StockPage/favorite/{stockName}", _jsonOptions);
        }

        public async Task<int> GetOwnedStocksAsync(string stockName, string? userCNP = null)
        {
            return await _httpClient.GetFromJsonAsync<int>($"api/StockPage/owned-stocks/{stockName}", _jsonOptions);
        }
        public async Task<List<decimal>> GetStockHistoryAsync(string stockName)
        {
            return await _httpClient.GetFromJsonAsync<List<decimal>>($"api/StockPage/history/{stockName}", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize stock history response.");
        }

        public async Task<UserStock> GetUserStockAsync(string stockName, string? userCNP = null)
        {
            return await _httpClient.GetFromJsonAsync<UserStock>($"api/StockPage/user-stock/{stockName}", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize user stock response.");
        }

        public async Task<bool> SellStockAsync(string stockName, int quantity, string? userCNP = null)
        {
            var response = await _httpClient.PostAsJsonAsync("api/StockPage/sell", new { stockName, quantity }, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
        }

        public async Task ToggleFavoriteAsync(string stockName, bool state, string? userCNP = null)
        {
            var response = await _httpClient.PostAsJsonAsync("api/StockPage/favorite/toggle", new { stockName, state }, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }
    }
}