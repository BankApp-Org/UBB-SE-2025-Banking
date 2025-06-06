using Common.Models.Bank;
using Common.Services.Bank;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class LoanProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IProxyService, ILoanService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task<List<Loan>> GetLoansAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Loan>>("api/Loan", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize loans response.");
        }

        public async Task<List<Loan>> GetUserLoansAsync(string userCNP)
        {
            if (string.IsNullOrEmpty(userCNP))
            {
                // Get current user's loans
                return await _httpClient.GetFromJsonAsync<List<Loan>>("api/Loan/user", _jsonOptions) ??
                    throw new InvalidOperationException("Failed to deserialize user loans response.");
            }
            else
            {
                // Get loans for a specific user (admin only)
                return await _httpClient.GetFromJsonAsync<List<Loan>>($"api/Loan/user/{userCNP}", _jsonOptions) ??
                    throw new InvalidOperationException("Failed to deserialize user loans response.");
            }
        }

        public async Task AddLoanAsync(LoanRequest loanRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Loan", loanRequest, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task CheckLoansAsync()
        {
            var response = await _httpClient.PostAsync("api/Loan/check", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task IncrementMonthlyPaymentsCompletedAsync(int loanID, decimal penalty)
        {
            var payment = new PaymentInfo { Penalty = penalty };
            var response = await _httpClient.PostAsJsonAsync($"api/Loan/{loanID}/increment-payment", payment, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateHistoryForUserAsync(string userCNP, int newScore)
        {
            var response = await _httpClient.PostAsync($"api/Loan/update-history?userCnp={userCNP}&newScore={newScore}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task PayLoanAsync(int loanId, decimal amount, string userCNP, string iban)
        {
            var paymentRequest = new PaymentDto
            {
                LoanId = loanId,
                Amount = amount,
                UserCNP = userCNP,
                Iban = iban,
            };
            var response = await _httpClient.PostAsJsonAsync($"api/Loan/PayLoan", paymentRequest, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }
    }

    public class PaymentInfo
    {
        public decimal Penalty { get; set; }
    }
    public class PaymentDto
    {
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public string UserCNP { get; set; }
        public string Iban { get; set; }
    }
}
