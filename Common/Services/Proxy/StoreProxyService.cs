using Common.Models.Trading;
using Common.Services.Trading;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class StoreProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IProxyService, IStoreService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task<string> BuyGems(GemDeal deal, string selectedAccountId, string? userCNP = null)
        {
            // The controller expects a GemDealDto which includes Title, GemAmount, Price, and SelectedAccountId.
            // userCNP is derived from claims on the server.
            var dealDto = new { deal.Title, deal.GemAmount, deal.Price, SelectedAccountId = selectedAccountId };
            var response = await _httpClient.PostAsJsonAsync("api/Store/buy-gems", dealDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<int> GetUserGemBalanceAsync(string? userCNP = null)
        {
            // userCNP is derived from claims on the server for this controller method.
            return await _httpClient.GetFromJsonAsync<int>("api/Store/user-gem-balance", _jsonOptions);
        }

        public async Task<string> SellGems(int gemAmount, string selectedAccountId, string? userCNP = null)
        {
            // The controller expects a SellGemsDto which includes GemAmount and SelectedAccountId.
            // userCNP is derived from claims on the server.
            var sellGemsDto = new { GemAmount = gemAmount, SelectedAccountId = selectedAccountId };
            var response = await _httpClient.PostAsJsonAsync("api/Store/sell-gems", sellGemsDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task UpdateUserGemBalanceAsync(int newBalance, string? userCNP = null)
        {
            // The controller expects an UpdateGemBalanceDto which includes UserCnp and NewBalance.
            // This method is Admin only, so userCNP must be provided if it's for another user.
            // If userCNP is null, it implies updating the current admin's balance, which might not be the intent.
            // For now, assume userCNP will be provided correctly by the caller for admin operations.
            var updateDto = new { UserCnp = userCNP, NewBalance = newBalance };
            var response = await _httpClient.PutAsJsonAsync("api/Store/user-gem-balance", updateDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }
    }
}