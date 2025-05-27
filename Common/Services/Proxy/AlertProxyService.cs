using Common.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class AlertProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IProxyService, IAlertService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task<Alert> CreateAlertAsync(string stockName, string name, decimal upperBound, decimal lowerBound, bool toggleOnOff)
        {
            var dto = new AlertCreateDto
            {
                StockName = stockName,
                Name = name,
                UpperBound = upperBound,
                LowerBound = lowerBound,
                ToggleOnOff = toggleOnOff
            };

            var response = await _httpClient.PostAsJsonAsync("api/Alert", dto, _jsonOptions);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Alert>(_jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize alert response.");
        }

        public async Task<Alert?> GetAlertByIdAsync(int alertId)
        {
            return await _httpClient.GetFromJsonAsync<Alert>($"api/Alert/{alertId}", _jsonOptions);
        }

        public async Task<List<Alert>> GetAllAlertsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Alert>>("api/Alert", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize alerts response.");
        }

        public async Task<List<Alert>> GetAllAlertsOnAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Alert>>("api/Alert/on", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize active alerts response.");
        }

        public async Task RemoveAlertAsync(int alertId)
        {
            var response = await _httpClient.DeleteAsync($"api/Alert/{alertId}");
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateAlertAsync(Alert alert)
        {
            var dto = new AlertUpdateDto
            {
                StockName = alert.StockName,
                Name = alert.Name,
                UpperBound = alert.UpperBound,
                LowerBound = alert.LowerBound,
                ToggleOnOff = alert.ToggleOnOff
            };

            var response = await _httpClient.PutAsJsonAsync($"api/Alert/{alert.AlertId}", dto, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateAlertAsync(int alertId, string stockName, string name, decimal upperBound, decimal lowerBound, bool toggleOnOff)
        {
            var dto = new AlertUpdateDto
            {
                StockName = stockName,
                Name = name,
                UpperBound = upperBound,
                LowerBound = lowerBound,
                ToggleOnOff = toggleOnOff
            };

            var response = await _httpClient.PutAsJsonAsync($"api/Alert/{alertId}", dto, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<TriggeredAlert>> GetTriggeredAlertsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<TriggeredAlert>>("api/Alert/triggered", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize triggered alerts response.");
        }
    }

    public class AlertCreateDto
    {
        public string StockName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal UpperBound { get; set; }
        public decimal LowerBound { get; set; }
        public bool ToggleOnOff { get; set; }
    }

    public class AlertUpdateDto
    {
        public string StockName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal UpperBound { get; set; }
        public decimal LowerBound { get; set; }
        public bool ToggleOnOff { get; set; }
    }
}