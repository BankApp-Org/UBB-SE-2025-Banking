using Common.Models.Social;
using Common.Services.Social;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Diagnostics;

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

            Debug.WriteLine($"Creating notification: {JsonSerializer.Serialize(dto, _jsonOptions)}");
            var response = await _httpClient.PostAsJsonAsync("api/Notification/notification", dto, _jsonOptions);
            Debug.WriteLine($"Create notification response: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Error response content: {errorContent}");
            }
            response.EnsureSuccessStatusCode();
        }

        public async Task<Notification> GetNotificationById(int notificationId)
        {
            Debug.WriteLine($"Getting notification by ID: {notificationId}");
            var response = await _httpClient.GetAsync($"api/Notification/{notificationId}");
            Debug.WriteLine($"Get notification by ID response: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Error response content: {errorContent}");
            }
            response.EnsureSuccessStatusCode();
            
            var notification = await response.Content.ReadFromJsonAsync<Notification>(_jsonOptions);
            Debug.WriteLine($"Received notification: {JsonSerializer.Serialize(notification, _jsonOptions)}");
            return notification ?? throw new InvalidOperationException("Failed to deserialize notification response.");
        }

        public async Task<List<Notification>> GetNotificationsForUser(int userId)
        {
            Debug.WriteLine($"Getting notifications for user ID: {userId}");
            Debug.WriteLine($"Request URL: {_httpClient.BaseAddress}api/Notification/user/{userId}");
            
            var response = await _httpClient.GetAsync($"api/Notification/user/{userId}");
            Debug.WriteLine($"Get notifications response: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Error response content: {errorContent}");
                response.EnsureSuccessStatusCode(); // This will throw with the full error details
            }

            var notifications = await response.Content.ReadFromJsonAsync<List<Notification>>(_jsonOptions);
            Debug.WriteLine($"Received notifications: {JsonSerializer.Serialize(notifications, _jsonOptions)}");
            return notifications ?? [];
        }

        public async Task MarkNotificationAsRead(int notificationId, int userId)
        {
            Debug.WriteLine($"Marking notification {notificationId} as read for user {userId}");
            var response = await _httpClient.PostAsync($"api/Notification/clear/{notificationId}", null);
            Debug.WriteLine($"Mark notification as read response: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Error response content: {errorContent}");
            }
            response.EnsureSuccessStatusCode();
        }

        public async Task MarkAllNotificationsAsRead(int userId)
        {
            Debug.WriteLine($"Marking all notifications as read for user {userId}");
            var response = await _httpClient.PostAsync($"api/Notification/clear-all/{userId}", null);
            Debug.WriteLine($"Mark all notifications as read response: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Error response content: {errorContent}");
            }
            response.EnsureSuccessStatusCode();
        }
    }
}