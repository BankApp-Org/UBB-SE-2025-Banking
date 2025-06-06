using Common.Models.Bank;
using Common.Services.Bank;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class BankAccountProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IProxyService, IBankAccountService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task<List<BankAccount>> GetUserBankAccounts(int userId)
        {
            return await _httpClient.GetFromJsonAsync<List<BankAccount>>($"api/BankAccount/user/{userId}", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize bank accounts response.");
        }

        public async Task<BankAccount> FindBankAccount(string iban)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<BankAccount>($"api/BankAccount/{iban}", _jsonOptions) ?? throw new Exception("Bank account deserialization failed");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception($"Account with iban: {iban} not found");
            }
        }

        public async Task<bool> CreateBankAccount(BankAccount bankAccount)
        {
            var request = new CreateBankAccountRequest
            {
                UserId = bankAccount.UserId,
                CustomName = bankAccount.Name,
                Currency = bankAccount.Currency.ToString()
            };

            var response = await _httpClient.PostAsJsonAsync("api/BankAccount", request, _jsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveBankAccount(string iban)
        {
            var response = await _httpClient.DeleteAsync($"api/BankAccount/{iban}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CheckIBANExists(string iban)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/BankAccount/{iban}/exists");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<string> GenerateIBAN()
        {
            var response = await _httpClient.GetStringAsync("api/BankAccount/generate-iban");
            return response;
        }

        public async Task<bool> UpdateBankAccount(BankAccount bankAccount)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/BankAccount/{bankAccount.Iban}", bankAccount, _jsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<decimal> ConvertCurrency(decimal amount, Currency fromCurrency, Currency toCurrency)
        {
            var response = await _httpClient.GetFromJsonAsync<CurrencyConversionResult>(
                $"api/BankAccount/convert?amount={amount}&fromCurrency={fromCurrency}&toCurrency={toCurrency}",
                _jsonOptions);

            return response?.ConvertedAmount ?? throw new InvalidOperationException("Failed to deserialize currency conversion response.");
        }

        public async Task<CurrencyExchange> GetExchangeRateAsync(Currency fromCurrency, Currency toCurrency)
        {
            return await _httpClient.GetFromJsonAsync<CurrencyExchange>(
                $"api/BankAccount/exchange-rate?fromCurrency={fromCurrency}&toCurrency={toCurrency}",
                _jsonOptions) ?? throw new InvalidOperationException("Failed to deserialize exchange rate response.");
        }

        public async Task<bool> CheckSufficientFunds(int userId, string accountIBAN, decimal amount, Currency currency)
        {
            var request = new CheckFundsRequest
            {
                UserId = userId,
                Iban = accountIBAN,
                Amount = amount,
                Currency = currency.ToString()
            };

            var response = await _httpClient.PostAsJsonAsync("api/BankAccount/check-funds", request, _jsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<CurrencyExchange>> GetAllExchangeRatesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<CurrencyExchange>>(
                $"api/BankAccount/ExchangeRates",
                _jsonOptions) ?? throw new InvalidOperationException("Failed to deserialize exchange rates response.");
        }
    }

    // DTO classes for API requests
    public class CreateBankAccountRequest
    {
        public int UserId { get; set; }
        public string? CustomName { get; set; }
        public string? Currency { get; set; }
    }

    public class CurrencyConversionResult
    {
        public decimal ConvertedAmount { get; set; }
    }

    public class CheckFundsRequest
    {
        public int UserId { get; set; }
        public string Iban { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}