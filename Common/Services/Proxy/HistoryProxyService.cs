using Common.Models;
using Common.Services.Bank;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class HistoryProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : ICreditHistoryService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task AddHistoryAsync(CreditScoreHistory history)
        {
            var response = await _httpClient.PostAsJsonAsync("api/history", history, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteHistoryAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/history/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<CreditScoreHistory>> GetAllHistoryAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<CreditScoreHistory>>("api/history", _jsonOptions);
            return response ?? [];
        }

        public async Task<CreditScoreHistory?> GetHistoryByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<CreditScoreHistory>($"api/history/{id}", _jsonOptions);
        }

        public async Task<List<CreditScoreHistory>> GetHistoryForUserAsync(string userCnp)
        {
            var response = await _httpClient.GetFromJsonAsync<List<CreditScoreHistory>>($"api/history/user/{userCnp}", _jsonOptions);
            return response ?? [];
        }

        public async Task<List<CreditScoreHistory>> GetHistoryMonthlyAsync(string? userCnp = null)
        {
            if (string.IsNullOrEmpty(userCnp))
            {
                // Current user
                var response = await _httpClient.GetFromJsonAsync<List<CreditScoreHistory>>("api/history/user/monthly", _jsonOptions);
                return response ?? [];
            }
            else
            {
                // Admin: get for specific user
                var response = await _httpClient.GetFromJsonAsync<List<CreditScoreHistory>>($"api/history/user/{userCnp}/monthly", _jsonOptions);
                return response ?? [];
            }
        }

        public async Task<List<CreditScoreHistory>> GetHistoryWeeklyAsync(string userCnp)
        {
            var response = await _httpClient.GetFromJsonAsync<List<CreditScoreHistory>>($"api/history/user/{userCnp}/weekly", _jsonOptions);
            return response ?? [];
        }

        public async Task<List<CreditScoreHistory>> GetHistoryYearlyAsync(string userCnp)
        {
            var response = await _httpClient.GetFromJsonAsync<List<CreditScoreHistory>>($"api/history/user/{userCnp}/yearly", _jsonOptions);
            return response ?? [];
        }

        public async Task UpdateHistoryAsync(CreditScoreHistory history)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/history/{history.Id}", history, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }
    }
}