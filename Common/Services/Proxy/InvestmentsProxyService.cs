using Common.Models.Bank;
using Common.Services.Bank;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class InvestmentsProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IInvestmentsService, IProxyService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task AddInvestmentAsync(Investment investment)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Investments", investment, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task CalculateAndUpdateROIAsync()
        {
            var response = await _httpClient.PostAsync("api/Investments/calculateROI", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task CalculateAndUpdateRiskScoreAsync()
        {
            var response = await _httpClient.PostAsync("api/Investments/calculateRiskScore", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task CreditScoreUpdateInvestmentsBasedAsync()
        {
            var response = await _httpClient.PostAsync("api/Investments/updateInvestmentsBasedOnCreditScore", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Investment>> GetInvestmentsHistoryAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Investment>>("api/Investments/history", _jsonOptions) ?? throw new InvalidOperationException("Failed to deserialize investments history response.");
        }

        public async Task<List<InvestmentPortfolio>> GetPortfolioSummaryAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<InvestmentPortfolio>>("api/Investments/portfolioSummary", _jsonOptions) ?? throw new InvalidOperationException("Failed to deserialize portfolio summary response.");
        }

        public async Task UpdateInvestmentAsync(int investmentId, string investorCNP, decimal amountReturned)
        {
            // The controller for UpdateInvestment takes investmentId in the route and amountReturned in the body.
            // investorCNP is derived from the token on the server-side.
            var response = await _httpClient.PutAsJsonAsync($"api/Investments/{investmentId}/update?amountReturned={amountReturned}", new { }, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }
    }
}