using Common.Models;
using Common.Models.Social;
using Common.Services.Social;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class ChatProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IProxyService, IChatService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task<bool> AddUserToChat(int chatId, User user)
        {
            if (user == null || string.IsNullOrEmpty(user.CNP))
                throw new ArgumentNullException(nameof(user), "User or user CNP cannot be null");

            var response = await _httpClient.PostAsync($"api/Chat/{chatId}/add-user/{user.CNP}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CreateChat(Chat chat)
        {
            if (chat == null)
                throw new ArgumentNullException(nameof(chat));

            var dto = new
            {
                ChatName = chat.ChatName,
                Participants = chat.Users
            };

            var response = await _httpClient.PostAsJsonAsync("api/Chat/create-chat", dto, _jsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task DeleteChat(int chatId)
        {
            var response = await _httpClient.DeleteAsync($"api/Chat/{chatId}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<Chat> GetChatById(int chatId)
        {
            return await _httpClient.GetFromJsonAsync<Chat>($"api/Chat/{chatId}", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize chat response.");
        }

        public async Task<int> GetNumberOfParticipants(int chatID)
        {
            return await _httpClient.GetFromJsonAsync<int>($"api/Chat/{chatID}/participants/count", _jsonOptions);
        }

        public async Task<List<Chat>> GetChatsForUser(int userId)
        {
            // Note: This endpoint might need to be implemented in the API
            return await _httpClient.GetFromJsonAsync<List<Chat>>($"api/Chat/user/{userId}", _jsonOptions) ??
                [];
        }

        public async Task<bool> RemoveUserFromChat(int chatId, User user)
        {
            if (user == null || string.IsNullOrEmpty(user.CNP))
                throw new ArgumentNullException(nameof(user), "User or user CNP cannot be null");

            var response = await _httpClient.DeleteAsync($"api/Chat/{chatId}/remove-user/{user.CNP}");
            return response.IsSuccessStatusCode;
        }

        public async Task UpdateChat(int chatId, Chat chat)
        {
            if (chat == null)
                throw new ArgumentNullException(nameof(chat));

            var response = await _httpClient.PutAsJsonAsync($"api/Chat/{chatId}", chat, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }
    }
}