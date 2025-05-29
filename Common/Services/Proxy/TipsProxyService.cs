using Common.Models;
using Common.Services.Social;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class TipsProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IProxyService, ITipsService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task GiveTipToUserAsync(string userCNP)
        {
            if (string.IsNullOrEmpty(userCNP))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCNP));
            }

            var response = await _httpClient.PostAsync($"api/Tips/user/{userCNP}/give", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Tip>> GetTipsForUserAsync(string userCnp)
        {
            // If userCnp is provided, get tips for the specified user (admin only)
            if (!string.IsNullOrEmpty(userCnp))
            {
                return await _httpClient.GetFromJsonAsync<List<Tip>>($"api/Tips/user/{userCnp}", _jsonOptions) ??
                    throw new InvalidOperationException("Failed to deserialize tips for user response.");
            }

            // If no userCnp is provided, get tips for the current user
            return await _httpClient.GetFromJsonAsync<List<Tip>>("api/Tips/user", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize tips response.");
        }
    }
}