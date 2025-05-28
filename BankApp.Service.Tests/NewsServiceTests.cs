using BankApi.Repositories;
using BankApi.Services;
using Common.Exceptions;
using Common.Models;
using Common.Models.Trading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace BankApp.Service.Tests
{
    [TestClass]
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class NewsServiceTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<INewsRepository> _mockNewsRepository;
        private NewsService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockNewsRepository = new Mock<INewsRepository>();
            _service = new NewsService(_mockUserRepository.Object, _mockNewsRepository.Object);
        }

        private NewsArticle CreateTestArticle(string id = "1", Status status = Status.Pending)
        {
            return new NewsArticle(
                articleId: id,
                title: $"Test Article {id}",
                summary: $"Summary {id}",
                content: $"Content {id}",
                source: "Test Source",
                topic: "Finance",
                publishedDate: DateTime.Now,
                relatedStocks: new List<Stock> { new Stock { Symbol = "AAPL", Price = 10, Quantity = 10 } },
                status: status);
        }

        [TestMethod]
        public async Task GetNewsArticlesAsync_ReturnsArticles_WhenRepositorySucceeds()
        {
            // Arrange
            var expectedArticles = new List<NewsArticle>
            {
                CreateTestArticle("1"),
                CreateTestArticle("2")
            };
            _mockNewsRepository.Setup(x => x.GetAllNewsArticlesAsync()).ReturnsAsync(expectedArticles);

            // Act
            var result = await _service.GetNewsArticlesAsync();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Test Article 1", result[0].Title);
            Assert.AreEqual("Finance", result[0].Topic);
            _mockNewsRepository.Verify(x => x.GetAllNewsArticlesAsync(), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(NewsPersistenceException))]
        public async Task GetNewsArticlesAsync_ThrowsException_WhenRepositoryFails()
        {
            // Arrange
            _mockNewsRepository.Setup(x => x.GetAllNewsArticlesAsync())
                .ThrowsAsync(new NewsPersistenceException("Database error"));

            // Act
            await _service.GetNewsArticlesAsync();
        }

        [TestMethod]
        public async Task GetNewsArticleByIdAsync_ReturnsArticle_WhenExists()
        {
            // Arrange
            var articleId = "123";
            var expectedArticle = CreateTestArticle(articleId);
            _mockNewsRepository.Setup(x => x.GetNewsArticleByIdAsync(articleId)).ReturnsAsync(expectedArticle);

            // Act
            var result = await _service.GetNewsArticleByIdAsync(articleId);

            // Assert
            Assert.AreEqual(expectedArticle, result);
            Assert.AreEqual("Finance", result.Topic);
            _mockNewsRepository.Verify(x => x.GetNewsArticleByIdAsync(articleId), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetNewsArticleByIdAsync_ThrowsException_WhenIdIsNull()
        {
            // Act
            await _service.GetNewsArticleByIdAsync(null);
        }

        [TestMethod]
        public async Task GetUserArticlesAsync_ReturnsFilteredArticles_WhenFiltersProvided()
        {
            // Arrange
            var authorCNP = "1234567890123";
            var user = new User { CNP = authorCNP };
            var articles = new List<NewsArticle>
            {
                CreateTestArticle("1", Status.Pending),
                CreateTestArticle("2", Status.Approved),
                CreateTestArticle("3", Status.Pending)
            };
            articles[0].Topic = "Tech";
            articles[1].Topic = "Finance";
            articles[2].Topic = "Finance";

            _mockUserRepository.Setup(x => x.GetByCnpAsync(authorCNP)).ReturnsAsync(user);
            _mockNewsRepository.Setup(x => x.GetNewsArticlesByAuthorCNPAsync(authorCNP)).ReturnsAsync(articles);

            // Act
            var result = await _service.GetUserArticlesAsync(Status.Pending, "Finance", authorCNP);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("3", result[0].ArticleId);
            Assert.AreEqual("Finance", result[0].Topic);
            _mockNewsRepository.Verify(x => x.GetNewsArticlesByAuthorCNPAsync(authorCNP), Times.Once);
        }

        [TestMethod]
        public async Task ApproveUserArticleAsync_UpdatesStatus_WhenSuccessful()
        {
            // Arrange
            var articleId = "123";
            var userCNP = "1234567890123";
            var user = new User { CNP = userCNP };
            var article = CreateTestArticle(articleId, Status.Pending);

            _mockUserRepository.Setup(x => x.GetByCnpAsync(userCNP)).ReturnsAsync(user);
            _mockNewsRepository.Setup(x => x.GetNewsArticleByIdAsync(articleId)).ReturnsAsync(article);
            _mockNewsRepository.Setup(x => x.UpdateNewsArticleAsync(article)).Returns(Task.CompletedTask);

            // Act
            var result = await _service.ApproveUserArticleAsync(articleId, userCNP);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(Status.Approved, article.Status);
            Assert.AreEqual("Finance", article.Topic);
            _mockNewsRepository.Verify(x => x.UpdateNewsArticleAsync(article), Times.Once);
        }


        [TestMethod]
        public async Task GetRelatedStocksForArticleAsync_ReturnsStocks_WhenArticleExists()
        {
            // Arrange
            var articleId = "123";
            var stocks = new List<Stock> { new Stock { Symbol = "AAPL", Price = 10, Quantity = 10 }, new Stock { Symbol = "MSFT", Price = 10, Quantity = 10 } };
            var article = CreateTestArticle(articleId);
            article.RelatedStocks = stocks;

            _mockNewsRepository.Setup(x => x.GetNewsArticleByIdAsync(articleId)).ReturnsAsync(article);

            // Act
            var result = await _service.GetRelatedStocksForArticleAsync(articleId);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("AAPL", result[0].Symbol);
            Assert.AreEqual("Finance", article.Topic);
            _mockNewsRepository.Verify(x => x.GetNewsArticleByIdAsync(articleId), Times.Once);
        }

        [TestMethod]
        public void NewsArticle_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var stocks = new List<Stock> { new Stock { Symbol = "TSLA", Price = 10, Quantity = 10 } };
            var publishDate = DateTime.Now;

            // Act
            var article = new NewsArticle(
                "123",
                "Test Title",
                "Test Summary",
                "Test Content",
                "Test Source",
                "Technology",
                publishDate,
                stocks,
                Status.Approved);

            // Assert
            Assert.AreEqual("123", article.ArticleId);
            Assert.AreEqual("Test Title", article.Title);
            Assert.AreEqual("Test Summary", article.Summary);
            Assert.AreEqual("Technology", article.Topic);
            Assert.AreEqual(Status.Approved, article.Status);
            Assert.AreEqual(1, article.RelatedStocks.Count);
            Assert.AreEqual("TSLA", article.RelatedStocks[0].Symbol);
        }

        [TestMethod]
        public void NewsArticle_DefaultConstructor_InitializesCollections()
        {
            // Act
            var article = new NewsArticle();

            // Assert
            Assert.IsNotNull(article.RelatedStocks);
            Assert.AreEqual(0, article.RelatedStocks.Count);
            Assert.AreEqual(string.Empty, article.Topic);
        }
    }
}