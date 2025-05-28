using Common.Models;
using Common.Models.Trading;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class NewsProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : INewsService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task<bool> ApproveUserArticleAsync(string articleId, string? userCNP = null)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/News/{articleId}/approve", userCNP, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
        }

        public async Task<bool> CreateArticleAsync(NewsArticle article, string? authorCNP = null)
        {
            var response = await _httpClient.PostAsJsonAsync("api/News/create", article, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
        }

        public async Task<bool> DeleteArticleAsync(string articleId)
        {
            var response = await _httpClient.DeleteAsync($"api/News/{articleId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
        }

        public async Task<NewsArticle> GetNewsArticleByIdAsync(string articleId)
        {
            return await _httpClient.GetFromJsonAsync<NewsArticle>($"api/News/{articleId}", _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize news article response.");
        }

        public async Task<List<NewsArticle>> GetNewsArticlesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<NewsArticle>>("api/News", _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize news articles response.");
        }

        public async Task<List<Stock>> GetRelatedStocksForArticleAsync(string articleId)
        {
            return await _httpClient.GetFromJsonAsync<List<Stock>>($"api/News/{articleId}/relatedstocks", _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize related stocks response.");
        }

        public async Task<List<NewsArticle>> GetUserArticlesAsync(Status status = Status.All, string topic = "All", string? authorCNP = null)
        {
            return await _httpClient.GetFromJsonAsync<List<NewsArticle>>($"api/News/userarticles?authorCnp={authorCNP}&status={status}&topic={topic}", _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize user articles response.");
        }

        public async Task<bool> MarkArticleAsReadAsync(string articleId)
        {
            var response = await _httpClient.PostAsync($"api/News/{articleId}/markasread", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
        }

        public async Task<bool> RejectUserArticleAsync(string articleId, string? userCNP = null)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/News/{articleId}/reject", userCNP, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
        }

        public async Task<bool> SubmitUserArticleAsync(NewsArticle article, string? userCNP = null)
        {
            var response = await _httpClient.PostAsJsonAsync("api/News/submit", article, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
        }
    }
}