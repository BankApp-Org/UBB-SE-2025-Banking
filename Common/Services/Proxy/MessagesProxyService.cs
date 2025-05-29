using Common.Models.Social;
using Common.Services.Social;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class MessagesProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IProxyService, IMessagesService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task GiveMessageToUserAsync(string userCNP)
        {
            if (string.IsNullOrEmpty(userCNP))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCNP));
            }

            var response = await _httpClient.PostAsync($"api/Messages/user/{userCNP}/give", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Message>> GetMessagesForUserAsync(string userCnp)
        {
            // If userCnp is provided, get messages for the specified user (admin only)
            if (!string.IsNullOrEmpty(userCnp))
            {
                return await _httpClient.GetFromJsonAsync<List<Message>>($"api/Messages/user/{userCnp}", _jsonOptions) ??
                    throw new InvalidOperationException("Failed to deserialize messages for user response.");
            }

            // If no userCnp is provided, get messages for the current user
            return await _httpClient.GetFromJsonAsync<List<Message>>("api/Messages/user", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize messages response.");
        }
        public async Task GiveMessageToUserAsync(string userCnp, string type, string messageText)
        {
            var request = new { Type = type, MessageText = messageText };
            await _httpClient.PostAsJsonAsync($"api/messages/user/{userCnp}/give", request, _jsonOptions);
        }
    }
}