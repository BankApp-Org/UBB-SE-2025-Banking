using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using System.Threading.Tasks;
using Common.Models.Bank;
using Common.Services.Bank;

namespace Common.Services.Proxy
{
    public class BankAccountProxyService : IBankAccountService

    {
        private readonly HttpClient _httpClient;

        public BankAccountProxyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<BankAccount>?> GetUserBankAccounts(int userID)
        {
            return await _httpClient.GetFromJsonAsync<List<BankAccount>>($"api/BankAccount/user/{userID}");
        }

        public async Task<BankAccount?> FindBankAccount(string iban)
        {
            return await _httpClient.GetFromJsonAsync<BankAccount>($"api/BankAccount/{iban}");
        }

        public async Task <bool> CreateBankAccount(BankAccount bankAccount)
        {
            var payload = new
            {
                UserId = bankAccount.UserId,
                CustomName = bankAccount.Name,
                Currency = bankAccount.Currency
            };
            var response =  await _httpClient.PostAsJsonAsync("api/BankAccount", payload);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveBankAccount(string iban)
        {
            var response = await _httpClient.DeleteAsync($"api/BankAccount/{iban}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CheckIBANExists(string iban)
        {
            var response = await _httpClient.GetAsync($"api/BankAccount/exists/{iban}");
            return response.IsSuccessStatusCode && bool.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task<string> GenerateIBAN()
        {
            return await _httpClient.GetStringAsync("api/BankAccount/generate-iban");
        }

        public async Task<List<string>> GetCurrencies()
        {
            return await _httpClient.GetFromJsonAsync<List<string>>("api/BankAccount/currencies");
        }

        public async Task<bool> VerifyUserCredentials(string email, string password)
        {
            var payload = new { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/BankAccount/verify", payload);
            return response.IsSuccessStatusCode && bool.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task <bool> UpdateBankAccount(BankAccount bankAccount)
        {
            var payload = new
            {
                Iban = bankAccount.Iban,
                Name = bankAccount.Name,
                DailyLimit = bankAccount.DailyLimit,
                MaxPerTransaction = bankAccount.MaximumPerTransaction,
                MaxNrTransactions = bankAccount.MaximumNrTransactions,
                Blocked = bankAccount.Blocked
            };

            var json = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"https://localhost:7097/api/bankaccount/{bankAccount.Iban}", json);
            return response.IsSuccessStatusCode;
        }

        public async Task<decimal> ConvertCurrency(decimal amount, Currency fromCurrency, Currency toCurrency)
        {
            try
            {
                var dto = new ConvertCurrencyDTO
                {
                    Amount = amount,
                    FromCurrency = fromCurrency,
                    ToCurrency = toCurrency,
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(dto),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/BankAccount/ConvertCurrency", content);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<decimal>(json);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting currency in proxy: {ex.Message}");
                throw new Exception($"Error converting currency in proxy: {ex.Message}");
            }
        }
        public async Task<CurrencyExchange> GetExchangeRateAsync(Currency fromCurrency, Currency toCurrency)
        {
            try
            {
                var payload = new
                {
                    FromCurrency = fromCurrency,
                    ToCurrency = toCurrency
                };

                var response = await _httpClient.PostAsJsonAsync("api/BankAccount/GetExchangeRate", payload);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CurrencyExchange>(json);

                return result!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching exchange rate: {ex.Message}");
                throw new Exception($"Error fetching exchange rate: {ex.Message}");
            }
        }

        public async Task<bool> CheckSufficientFunds(int userID, string accountIBAN, decimal amount, Currency currency)
        {
            try
            {
                var payload = new
                {
                    UserId = userID,
                    AccountIban = accountIBAN,
                    Amount = amount,
                    Currency = currency
                };

                var response = await _httpClient.PostAsJsonAsync("api/BankAccount/CheckSufficientFunds", payload);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<bool>(json);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking funds: {ex.Message}");
                throw new Exception($"Error checking funds: {ex.Message}");
            }
        }

    }


}

public class ConvertCurrencyDTO
{
    public decimal Amount { get; set; }

    public Currency FromCurrency { get; set; }

    public Currency ToCurrency { get; set; }
}

