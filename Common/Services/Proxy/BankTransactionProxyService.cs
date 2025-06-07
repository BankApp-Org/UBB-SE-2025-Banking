using Common.Models.Bank;
using Common.Services.Bank;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class BankTransactionProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IProxyService, IBankTransactionService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task<List<BankTransaction>> GetTransactions(TransactionFilters transactionFilters)
        {
            //var queryString = BuildQueryString(transactionFilters);
            var response =
                await _httpClient.PostAsJsonAsync("api/TransactionHistory", transactionFilters, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<BankTransaction>>(_jsonOptions) ??
                   throw new InvalidOperationException("Failed to deserialize bank transactions response.");
        }

        private string BuildQueryString(TransactionFilters filters)
        {
            var queryParams = new List<string>
            {
                $"type={filters.Type}",
                $"startDate={Uri.EscapeDataString(filters.StartDate.ToString("o"))}",
                $"endDate={Uri.EscapeDataString(filters.EndDate.ToString("o"))}",
                $"ordering={filters.Ordering}"
            };

            if (!string.IsNullOrEmpty(filters.SenderIban))
                queryParams.Add($"senderIban={Uri.EscapeDataString(filters.SenderIban)}");

            if (!string.IsNullOrEmpty(filters.ReceiverIban))
                queryParams.Add($"receiverIban={Uri.EscapeDataString(filters.ReceiverIban)}");

            if (filters.MinAmount.HasValue)
                queryParams.Add($"minAmount={filters.MinAmount.Value}");

            if (filters.MaxAmount.HasValue)
                queryParams.Add($"maxAmount={filters.MaxAmount.Value}");

            return queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
        }

        public async Task<BankTransaction?> GetTransactionById(int transactionId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<BankTransaction>($"api/BankTransaction/{transactionId}", _jsonOptions);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<bool> CreateTransaction(BankTransaction transaction)
        {
            var response = await _httpClient.PostAsJsonAsync("api/BankTransaction/AddTransaction", transaction, _jsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTransaction(BankTransaction transaction)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/BankTransaction/{transaction.TransactionId}", transaction, _jsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTransaction(int transactionId)
        {
            var response = await _httpClient.DeleteAsync($"api/BankTransaction/{transactionId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CreateCSV(string IBAN)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/TransactionHistory/CreateCSV/{IBAN}", null);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating CSV file", ex);
            }
        }

        public async Task<string> GenerateCsvStringAsync(string iban)
        {
            var response = await _httpClient.GetAsync($"api/TransactionHistory/ExportCsvString/{iban}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }


        public async Task<List<TransactionTypeCountDTO>> GetTransactionTypeCounts(int userId)
        {
            return await _httpClient.GetFromJsonAsync<List<TransactionTypeCountDTO>>($"api/BankTransaction/TransactionTypeCounts?userId={userId}", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize transaction type counts response.");
        }
    }
}

