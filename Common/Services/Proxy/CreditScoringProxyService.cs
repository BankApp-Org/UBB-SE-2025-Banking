using Common.Models;
using Common.Models.Bank;
using Common.Models.Trading;
using Common.Services.Bank;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class CreditScoringProxyService : ICreditScoringService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public CreditScoringProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsonOptions = jsonOptions?.Value?.SerializerOptions ?? new JsonSerializerOptions();
        }

        public async Task<int> CalculateBankTransactionImpactAsync(string userCnp, BankTransaction transaction)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/creditscore/bank-transaction-impact/{userCnp}", transaction, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<int>(result, _jsonOptions);
        }

        public async Task<int> CalculateLoanPaymentImpactAsync(string userCnp, Loan loan, decimal paymentAmount, bool isOnTime)
        {
            var request = new { Loan = loan, PaymentAmount = paymentAmount, IsOnTime = isOnTime };
            var response = await _httpClient.PostAsJsonAsync($"api/creditscore/loan-payment-impact/{userCnp}", request, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<int>(result, _jsonOptions);
        }

        public async Task<int> CalculateGemTransactionImpactAsync(string userCnp, bool isBuying, int gemAmount, double transactionValue)
        {
            var request = new { IsBuying = isBuying, GemAmount = gemAmount, TransactionValue = transactionValue };
            var response = await _httpClient.PostAsJsonAsync($"api/creditscore/gem-transaction-impact/{userCnp}", request, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<int>(result, _jsonOptions);
        }

        public async Task<int> CalculateStockTransactionImpactAsync(string userCnp, StockTransaction stockTransaction)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/creditscore/stock-transaction-impact/{userCnp}", stockTransaction, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<int>(result, _jsonOptions);
        }

        public async Task UpdateCreditScoreAsync(string userCnp, int newScore, string reason)
        {
            var request = new { NewScore = newScore, Reason = reason };
            var response = await _httpClient.PostAsJsonAsync($"api/creditscore/update/{userCnp}", request, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task<int> GetCurrentCreditScoreAsync(string userCnp)
        {
            var response = await _httpClient.GetAsync($"api/creditscore/current/{userCnp}");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<int>(result, _jsonOptions);
        }

        public async Task<int> CalculateComprehensiveCreditScoreAsync(string userCnp)
        {
            var response = await _httpClient.GetAsync($"api/creditscore/comprehensive/{userCnp}");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<int>(result, _jsonOptions);
        }
    }
}