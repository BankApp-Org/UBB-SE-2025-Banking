using Common.Models;
using Common.Models.Trading;
using Common.Services.Trading;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace Common.Services.Proxy
{
    public class StockProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IProxyService, IStockService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly JsonSerializerOptions _options = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task<Stock> CreateStockAsync(Stock stock)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Stock", stock, _options);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Stock>(_options) ?? throw new InvalidOperationException("Failed to deserialize stock response.");
        }

        public async Task<bool> DeleteStockAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Stock/{id}");
            response.EnsureSuccessStatusCode();
            // Assuming API returns true/false in body for now.
            return response.StatusCode == System.Net.HttpStatusCode.NoContent || await response.Content.ReadFromJsonAsync<bool>(_options);
        }

        public async Task<IEnumerable<Stock>> GetAllStocksAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Stock>>("api/Stock", _options) ?? throw new InvalidOperationException("Failed to deserialize stocks response.");
        }

        public async Task<Stock?> GetStockByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Stock?>($"api/Stock/{id}", _options);
        }

        public async Task<Stock?> GetStockByNameAsync(string name)
        {
            var allStocks = await this.GetAllStocksAsync();
            return allStocks.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<Stock?> UpdateStockAsync(int id, Stock updatedStock)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Stock/{id}", updatedStock, _options);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Stock?>(_options);
        }

        public async Task<List<Stock>> UserStocksAsync(string? userCNP = null)
        {
            string endpoint = string.IsNullOrEmpty(userCNP) ? "api/Stock/user" : $"api/Stock/user/{userCNP}";
            return await _httpClient.GetFromJsonAsync<List<Stock>>(endpoint, _options) ?? [];
        }

        public async Task AddToFavoritesAsync(HomepageStock stock)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Stock/favorites/add", stock, _options);
            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveFromFavoritesAsync(HomepageStock stock)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Stock/favorites/remove", stock, _options);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<HomepageStock>> GetFilteredAndSortedStocksAsync(string query, string sortOption, bool favoritesOnly, string? userCNP = null)
        {
            var uriBuilder = new UriBuilder(_httpClient.BaseAddress + "api/Stock/stocks");
            var queryParams = HttpUtility.ParseQueryString(uriBuilder.Query);
            queryParams["query"] = query;
            queryParams["sortOption"] = sortOption;
            queryParams["favoritesOnly"] = favoritesOnly.ToString();
            // userCNP is not sent as a query parameter as the controller resolves it from the authenticated user.
            uriBuilder.Query = queryParams.ToString();
            return await _httpClient.GetFromJsonAsync<List<HomepageStock>>(uriBuilder.ToString(), _options) ?? throw new InvalidOperationException("Failed to deserialize filtered and sorted stocks response.");
        }
    }

}
