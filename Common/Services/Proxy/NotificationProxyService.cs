using Common.Models.Social;
using Common.Services.Social;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class NotificationProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IProxyService, INotificationService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task CreateNotification(Notification notification)
        {
            var dto = new
            {
                NotificationID = notification.NotificationID,
                Timestamp = notification.Timestamp,
                Content = notification.Content,
                UserReceiver = notification.User
            };

            var response = await _httpClient.PostAsJsonAsync("api/Notification/notification", dto, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task<Notification> GetNotificationById(int notificationId)
        {
            return await _httpClient.GetFromJsonAsync<Notification>($"api/Notification/{notificationId}", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize notification response.");
        }

        public async Task<List<Notification>> GetNotificationsForUser(int userId)
        {
            return await _httpClient.GetFromJsonAsync<List<Notification>>($"api/Notification/user/{userId}", _jsonOptions) ??
                [];
        }

        public async Task MarkAllNotificationsAsRead(int userId)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Notification/clear-all", userId, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task MarkNotificationAsRead(int notificationId, int userId)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Notification/clear", notificationId, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }
    }
}