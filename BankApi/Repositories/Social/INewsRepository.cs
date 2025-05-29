using Common.Models;

namespace BankApi.Repositories.Social
{
    public interface INewsRepository
    {
        Task AddNewsArticleAsync(NewsArticle newsArticle);
        Task DeleteNewsArticleAsync(string articleId);
        Task<List<NewsArticle>> GetAllNewsArticlesAsync();
        Task<List<NewsArticle>> GetNewsArticlesByAuthorCNPAsync(string authorCNP);
        Task<NewsArticle> GetNewsArticleByIdAsync(string articleId);
        Task<List<NewsArticle>> GetNewsArticlesByCategoryAsync(string category);
        Task<List<NewsArticle>> GetNewsArticlesByStockAsync(string stockName);
        Task MarkArticleAsReadAsync(string articleId);
        Task UpdateNewsArticleAsync(NewsArticle newsArticle);
    }
}