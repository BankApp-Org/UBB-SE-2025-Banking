using Common.Models.Social;
using Common.Services.Social;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class ChatReportProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IProxyService, IChatReportService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task<List<ChatReport>> GetAllChatReportsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ChatReport>>("api/ChatReport", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize chat reports response.");
        }

        public async Task<ChatReport?> GetChatReportByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ChatReport>($"api/ChatReport/{id}", _jsonOptions);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task AddChatReportAsync(ChatReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report), "Chat report cannot be null");
            }

            var response = await _httpClient.PostAsJsonAsync("api/ChatReport", report, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteChatReportAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/ChatReport/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<int> GetNumberOfGivenTipsForUserAsync(string? userCnp = null)
        {
            string endpoint = string.IsNullOrEmpty(userCnp)
                ? "api/ChatReport/user-tips/current"
                : $"api/ChatReport/user-tips/{userCnp}";

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<int>(_jsonOptions);
        }

        public async Task UpdateActivityLogAsync(int amount, string? userCnp = null)
        {
            var updateDto = new ActivityLogUpdateDto { Amount = amount };
            var response = await _httpClient.PostAsJsonAsync("api/ChatReport/activity-log", updateDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateScoreHistoryForUserAsync(int newScore, string? userCnp = null)
        {
            var updateDto = new ScoreHistoryUpdateDto { NewScore = newScore };
            var response = await _httpClient.PostAsJsonAsync("api/ChatReport/score-history", updateDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        // Implementing the new interface methods
        public async Task PunishUser(ChatReport chatReportToBeSolved)
        {
            if (chatReportToBeSolved == null)
            {
                throw new ArgumentNullException(nameof(chatReportToBeSolved), "Chat report cannot be null");
            }

            var punishmentDto = new PunishmentMessageDto
            {
                ShouldPunish = true,
                MessageContent = "Your message was reported and found to be in violation of our community standards."
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"api/ChatReport/punish-with-message/{chatReportToBeSolved.Id}",
                punishmentDto, _jsonOptions);

            response.EnsureSuccessStatusCode();
        }

        public async Task DoNotPunishUser(ChatReport chatReportToBeSolved)
        {
            if (chatReportToBeSolved == null)
            {
                throw new ArgumentNullException(nameof(chatReportToBeSolved), "Chat report cannot be null");
            }

            var punishmentDto = new PunishmentMessageDto
            {
                ShouldPunish = false,
                MessageContent = string.Empty // No message needed since we're not punishing
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"api/ChatReport/punish-with-message/{chatReportToBeSolved.Id}",
                punishmentDto, _jsonOptions);

            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> IsMessageOffensive(Message messageToBeChecked)
        {
            if (string.IsNullOrEmpty(messageToBeChecked.MessageContent))
            {
                return false;
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "api/ChatReport/check-message",
                    new { Message = messageToBeChecked }, _jsonOptions);

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
            }
            catch
            {
                // If the API call fails, assume the message is not offensive
                return false;
            }
        }

        // Send a message to a user
        public async Task SendMessageToUser(string userCnp, string messageContent, string messageType = "System")
        {
            if (string.IsNullOrEmpty(userCnp))
            {
                throw new ArgumentException("User CNP cannot be null or empty", nameof(userCnp));
            }

            if (string.IsNullOrEmpty(messageContent))
            {
                throw new ArgumentException("Message content cannot be null or empty", nameof(messageContent));
            }

            var messageDto = new SendMessageDto
            {
                UserCnp = userCnp,
                MessageType = messageType,
                MessageContent = messageContent
            };

            var response = await _httpClient.PostAsJsonAsync("api/ChatReport/send-message", messageDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> CheckIfReportExists(int messageId, int reporterUserId)
        {
            if (messageId <= 0 || reporterUserId <= 0)
            {
                throw new ArgumentException("Message ID and Reporter User ID must be greater than zero.");
            }
            try
            {
                var response = await _httpClient.GetAsync($"api/ChatReport/check-exists/{messageId}/{reporterUserId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false; // Report does not exist
            }
        }
    }

    // DTOs needed for API calls
    internal class ActivityLogUpdateDto
    {
        public int Amount { get; set; }
    }

    internal class ScoreHistoryUpdateDto
    {
        public int NewScore { get; set; }
    }

    internal class PunishmentMessageDto
    {
        public bool ShouldPunish { get; set; } = true;
        public string MessageContent { get; set; } = string.Empty;
    }

    internal class SendMessageDto
    {
        public string UserCnp { get; set; } = string.Empty;
        public string MessageType { get; set; } = "System";
        public string MessageContent { get; set; } = string.Empty;
    }
}