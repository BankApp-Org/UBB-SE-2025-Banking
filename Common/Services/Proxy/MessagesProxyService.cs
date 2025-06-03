using Common.Models;
using Common.Models.Social;
using Common.Services.Social;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class MessagesProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IProxyService, IMessageService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task<Message> SendMessageAsync(int chatId, User sender, Message message)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Messages/{chatId}", message, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Message>(_jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize send message response.");
        }

        public async Task DeleteMessageAsync(int chatId, int messageId, User requester)
        {
            var response = await _httpClient.DeleteAsync($"api/Messages/{chatId}/{messageId}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<Message> GetMessageByIdAsync(int messageId)
        {
            return await _httpClient.GetFromJsonAsync<Message>($"api/Messages/{messageId}", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize message response.");
        }

        public async Task<List<Message>> GetMessagesAsync(int chatId, int page, int pageSize)
        {
            return await _httpClient.GetFromJsonAsync<List<Message>>($"api/Messages/{chatId}?page={page}&pageSize={pageSize}", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize messages response.");
        }

        public async Task ReportMessage(int chatId, int messageId, User reporter, ReportReason reason)
        {
            var report = new { ReportReason = reason };
            var response = await _httpClient.PostAsJsonAsync($"api/Messages/{chatId}/{messageId}/report", report, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

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