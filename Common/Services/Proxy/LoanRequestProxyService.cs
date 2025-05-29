using Common.Models.Bank;
using Common.Services.Bank;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class LoanRequestProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IProxyService, ILoanRequestService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");


        public async Task<List<LoanRequest>> GetLoanRequests()
        {
            return await _httpClient.GetFromJsonAsync<List<LoanRequest>>("api/LoanRequest", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize loan requests response.");
        }

        public async Task<List<LoanRequest>> GetUnsolvedLoanRequests()
        {
            return await _httpClient.GetFromJsonAsync<List<LoanRequest>>("api/LoanRequest/unsolved", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize unsolved loan requests response.");
        }

        public async Task<string> GiveSuggestion(LoanRequest loanRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/LoanRequest/suggestion", loanRequest, _jsonOptions);
            response.EnsureSuccessStatusCode();

            string suggestion = await response.Content.ReadAsStringAsync() ??
                throw new InvalidOperationException("Failed to deserialize suggestion response.");

            return suggestion;
        }

        public async Task SolveLoanRequest(int loanRequestId)
        {
            var response = await _httpClient.PostAsync($"api/LoanRequest/{loanRequestId}/solve", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteLoanRequest(int loanRequestId)
        {
            var response = await _httpClient.DeleteAsync($"api/LoanRequest/{loanRequestId}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<LoanRequest> CreateLoanRequest(LoanRequest loanRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/LoanRequest", loanRequest, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LoanRequest>(_jsonOptions) ??
                   throw new InvalidOperationException("Failed to deserialize create loan request response.");
        }
    }
}