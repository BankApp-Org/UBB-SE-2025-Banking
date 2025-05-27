namespace BankApi.Repositories.Impl
{
    using BankApi.Data;
    using Common.Models;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class NewsRepository(ApiDbContext dbContext) : INewsRepository
    {
        private readonly ApiDbContext _dbContext = dbContext;

        public async Task AddNewsArticleAsync(NewsArticle newsArticle)
        {
            try
            {
                // Ensure RelatedStocks are properly tracked
                var relatedStocks = new List<Stock>();
                foreach (var stock in newsArticle.RelatedStocks.ToList())
                {
                    var trackedStock = _dbContext.Stocks.Local
                        .FirstOrDefault(s => s.Name == stock.Name);

                    if (trackedStock != null)
                    {
                        relatedStocks.Add(trackedStock);
                    }
                    else
                    {
                        var existingStock = await _dbContext.Stocks
                            .FirstOrDefaultAsync(s => s.Name == stock.Name);

                        if (existingStock != null)
                        {
                            _dbContext.Entry(existingStock).State = EntityState.Unchanged;
                            relatedStocks.Add(existingStock);
                        }
                        else
                        {
                            throw new Exception($"Stock with name {stock.Name} does not exist in the database.");
                        }
                    }
                }
                newsArticle.RelatedStocks = relatedStocks;

                // Ensure Author is properly tracked
                var trackedAuthor = _dbContext.Users.Local
                    .FirstOrDefault(u => u.Id == newsArticle.Author.Id);

                if (trackedAuthor != null)
                {
                    newsArticle.Author = trackedAuthor;
                }
                else
                {
                    var existingAuthor = await _dbContext.Users
                        .FirstOrDefaultAsync(u => u.Id == newsArticle.Author.Id);

                    if (existingAuthor != null)
                    {
                        _dbContext.Entry(existingAuthor).State = EntityState.Unchanged;
                        newsArticle.Author = existingAuthor;
                    }
                    else
                    {
                        throw new Exception($"Author with ID {newsArticle.Author.Id} does not exist in the database.");
                    }
                }

                // Ensure ArticleId is set
                if (string.IsNullOrEmpty(newsArticle.ArticleId))
                {
                    newsArticle.ArticleId = Guid.NewGuid().ToString();
                }

                // Check if the article already exists
                var existingArticle = await _dbContext.NewsArticles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.ArticleId == newsArticle.ArticleId);

                if (existingArticle != null)
                {
                    await UpdateNewsArticleAsync(newsArticle);
                    return;
                }

                // Add the new article
                await _dbContext.NewsArticles.AddAsync(newsArticle);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while adding news article.", ex);
            }
        }
        public async Task UpdateNewsArticleAsync(NewsArticle newsArticle)
        {
            try
            {
                var existingArticle = await _dbContext.NewsArticles
                    .FirstOrDefaultAsync(a => a.ArticleId == newsArticle.ArticleId) ?? throw new KeyNotFoundException($"Article with ID {newsArticle.ArticleId} not found.");
                _dbContext.Entry(existingArticle).CurrentValues.SetValues(newsArticle);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating news article.", ex);
            }
        }

        public async Task DeleteNewsArticleAsync(string articleId)
        {
            try
            {
                var article = await _dbContext.NewsArticles
                    .FirstOrDefaultAsync(a => a.ArticleId == articleId) ?? throw new KeyNotFoundException($"Article with ID {articleId} not found.");
                _dbContext.NewsArticles.Remove(article);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while deleting news article.", ex);
            }
        }

        public async Task<NewsArticle> GetNewsArticleByIdAsync(string articleId)
        {
            try
            {
                var article = await _dbContext.NewsArticles
                    .Include(a => a.RelatedStocks)
                    .Include(u => u.Author)
                    .FirstOrDefaultAsync(a => a.ArticleId == articleId);

                return article ?? throw new KeyNotFoundException($"Article with ID {articleId} not found.");
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving news article.", ex);
            }
        }

        public async Task<List<NewsArticle>> GetAllNewsArticlesAsync()
        {
            try
            {
                return await _dbContext.NewsArticles
                    .Include(a => a.RelatedStocks)
                    .Include(u => u.Author)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving all news articles.", ex);
            }
        }

        public async Task<List<NewsArticle>> GetNewsArticlesByAuthorCNPAsync(string authorCNP)
        {
            try
            {
                if (string.IsNullOrEmpty(authorCNP))
                {
                    return await _dbContext.NewsArticles
                        .Include(a => a.RelatedStocks)
                        .Include(u => u.Author)
                        .ToListAsync();
                }
                else
                {
                    return await _dbContext.NewsArticles
                        .Where(a => a.AuthorCNP == authorCNP)
                        .Include(a => a.RelatedStocks)
                        .Include(u => u.Author)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving news articles by author CNP.", ex);
            }
        }

        public async Task<List<NewsArticle>> GetNewsArticlesByCategoryAsync(string category)
        {
            try
            {
                return await _dbContext.NewsArticles
                    .Where(a => a.Category == category)
                    .Include(a => a.RelatedStocks)
                    .Include(u => u.Author)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving news articles by category.", ex);
            }
        }

        public async Task<List<NewsArticle>> GetNewsArticlesByStockAsync(string stockName)
        {
            try
            {
                return await _dbContext.NewsArticles
                    .Where(a => a.RelatedStocks.Any(rs => rs.Name == stockName))
                    .Include(a => a.RelatedStocks)
                    .Include(u => u.Author)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving news articles by stock.", ex);
            }
        }

        public async Task MarkArticleAsReadAsync(string articleId)
        {
            try
            {
                var article = await GetNewsArticleByIdAsync(articleId);
                article.IsRead = true;
                await UpdateNewsArticleAsync(article);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while marking article as read.", ex);
            }
        }
    }
}