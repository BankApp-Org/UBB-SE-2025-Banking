using Common.Models.Bank;
using Common.Services.Bank;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class ActivityProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IActivityService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task<List<ActivityLog>> GetActivityForUser(string userCNP)
        {
            var response = await _httpClient.GetFromJsonAsync<List<ActivityLog>>($"api/Activity/user/{userCNP}", _jsonOptions);
            return response ?? [];
        }

        public async Task<ActivityLog> AddActivity(string userCnp, string activityName, int amount, string details)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Activity", new
            {
                UserCnp = userCnp,
                ActivityName = activityName,
                LastModifiedAmount = amount,
                ActivityDetails = details
            }, _jsonOptions);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ActivityLog>(_jsonOptions) ?? throw new Exception("Failed to deserialize activity log");
        }

        public async Task<List<ActivityLog>> GetAllActivities()
        {
            var response = await _httpClient.GetFromJsonAsync<List<ActivityLog>>("api/Activity", _jsonOptions);
            return response ?? [];
        }

        public async Task<ActivityLog> GetActivityById(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ActivityLog>($"api/Activity/{id}", _jsonOptions);
            return response ?? throw new Exception($"Activity with ID {id} not found");
        }

        public async Task<bool> DeleteActivity(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Activity/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}