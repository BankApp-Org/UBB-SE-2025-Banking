using BankApi.Repositories.Trading;
using BankApi.Services.Trading;
using Common.Models;
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
    public class HistoryServiceTests
    {
        private Mock<IStockHistoryRepository> _mockRepo;
        private HistoryService _service;

        [TestInitialize]
        public void Init()
        {
            _mockRepo = new Mock<IStockHistoryRepository>();
            _service = new HistoryService(_mockRepo.Object);
        }

        [TestMethod]
        public async Task GetAllHistoryAsync_HappyCase_ReturnsList()
        {
            var history = new List<CreditScoreHistory> { new() { Id = 1, Score = 500 } };
            _mockRepo.Setup(r => r.GetAllHistoryAsync()).ReturnsAsync(history);
            var result = await _service.GetAllHistoryAsync();
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetAllHistoryAsync_RepositoryThrows_ThrowsException()
        {
            _mockRepo.Setup(r => r.GetAllHistoryAsync()).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Common.Exceptions.HistoryServiceException>(async () => await _service.GetAllHistoryAsync());
        }

        [TestMethod]
        public async Task GetHistoryByIdAsync_HappyCase_ReturnsHistory()
        {
            var history = new CreditScoreHistory { Id = 1, Score = 500 };
            _mockRepo.Setup(r => r.GetHistoryByIdAsync(1)).ReturnsAsync(history);
            var result = await _service.GetHistoryByIdAsync(1);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
        }

        [TestMethod]
        public async Task GetHistoryByIdAsync_RepositoryThrows_ThrowsException()
        {
            _mockRepo.Setup(r => r.GetHistoryByIdAsync(1)).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Common.Exceptions.HistoryServiceException>(async () => await _service.GetHistoryByIdAsync(1));
        }

        [TestMethod]
        public async Task AddHistoryAsync_HappyCase_AddsHistory()
        {
            var history = new CreditScoreHistory { Id = 1, Score = 500 };
            _mockRepo.Setup(r => r.AddHistoryAsync(history)).Returns(Task.FromResult(history));
            await _service.AddHistoryAsync(history);
            _mockRepo.Verify(r => r.AddHistoryAsync(history), Times.Once);
        }

        [TestMethod]
        public async Task AddHistoryAsync_NullHistory_ThrowsArgumentNullException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await _service.AddHistoryAsync(null));
        }

        [TestMethod]
        public async Task AddHistoryAsync_RepositoryThrows_ThrowsException()
        {
            var history = new CreditScoreHistory { Id = 1, Score = 500 };
            _mockRepo.Setup(r => r.AddHistoryAsync(history)).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Common.Exceptions.HistoryServiceException>(async () => await _service.AddHistoryAsync(history));
        }

        [TestMethod]
        public async Task UpdateHistoryAsync_HappyCase_UpdatesHistory()
        {
            var history = new CreditScoreHistory { Id = 1, Score = 500 };
            _mockRepo.Setup(r => r.UpdateHistoryAsync(history)).Returns(Task.FromResult(history));
            await _service.UpdateHistoryAsync(history);
            _mockRepo.Verify(r => r.UpdateHistoryAsync(history), Times.Once);
        }

        [TestMethod]
        public async Task UpdateHistoryAsync_NullHistory_ThrowsArgumentNullException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await _service.UpdateHistoryAsync(null));
        }

        [TestMethod]
        public async Task UpdateHistoryAsync_RepositoryThrows_ThrowsException()
        {
            var history = new CreditScoreHistory { Id = 1, Score = 500 };
            _mockRepo.Setup(r => r.UpdateHistoryAsync(history)).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Common.Exceptions.HistoryServiceException>(async () => await _service.UpdateHistoryAsync(history));
        }

        [TestMethod]
        public async Task DeleteHistoryAsync_HappyCase_DeletesHistory()
        {
            _mockRepo.Setup(r => r.DeleteHistoryAsync(1)).Returns(Task.CompletedTask);
            await _service.DeleteHistoryAsync(1);
            _mockRepo.Verify(r => r.DeleteHistoryAsync(1), Times.Once);
        }

        [TestMethod]
        public async Task DeleteHistoryAsync_RepositoryThrows_ThrowsException()
        {
            _mockRepo.Setup(r => r.DeleteHistoryAsync(1)).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Common.Exceptions.HistoryServiceException>(async () => await _service.DeleteHistoryAsync(1));
        }

        [TestMethod]
        public async Task GetHistoryForUserAsync_HappyCase_ReturnsList()
        {
            var history = new List<CreditScoreHistory> { new() { Id = 1, Score = 500 } };
            _mockRepo.Setup(r => r.GetHistoryForUserAsync("123")).ReturnsAsync(history);
            var result = await _service.GetHistoryForUserAsync("123");
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetHistoryForUserAsync_EmptyCnp_ThrowsArgumentException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _service.GetHistoryForUserAsync(""));
        }

        [TestMethod]
        public async Task GetHistoryForUserAsync_RepositoryThrows_ThrowsException()
        {
            _mockRepo.Setup(r => r.GetHistoryForUserAsync("123")).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Common.Exceptions.HistoryServiceException>(async () => await _service.GetHistoryForUserAsync("123"));
        }

        [TestMethod]
        public async Task GetHistoryWeeklyAsync_HappyCase_ReturnsList()
        {
            var history = new List<CreditScoreHistory> { new() { Id = 1, Score = 500 } };
            _mockRepo.Setup(r => r.GetHistoryWeeklyAsync("123")).ReturnsAsync(history);
            var result = await _service.GetHistoryWeeklyAsync("123");
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetHistoryWeeklyAsync_EmptyCnp_ThrowsArgumentException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _service.GetHistoryWeeklyAsync(""));
        }

        [TestMethod]
        public async Task GetHistoryWeeklyAsync_RepositoryThrows_ThrowsException()
        {
            _mockRepo.Setup(r => r.GetHistoryWeeklyAsync("123")).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Common.Exceptions.HistoryServiceException>(async () => await _service.GetHistoryWeeklyAsync("123"));
        }

        [TestMethod]
        public async Task GetHistoryMonthlyAsync_HappyCase_ReturnsList()
        {
            var history = new List<CreditScoreHistory> { new() { Id = 1, Score = 500 } };
            _mockRepo.Setup(r => r.GetHistoryMonthlyAsync("123")).ReturnsAsync(history);
            var result = await _service.GetHistoryMonthlyAsync("123");
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetHistoryMonthlyAsync_EmptyCnp_ThrowsArgumentException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _service.GetHistoryMonthlyAsync(""));
        }

        [TestMethod]
        public async Task GetHistoryMonthlyAsync_RepositoryThrows_ThrowsException()
        {
            _mockRepo.Setup(r => r.GetHistoryMonthlyAsync("123")).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Common.Exceptions.HistoryServiceException>(async () => await _service.GetHistoryMonthlyAsync("123"));
        }

        [TestMethod]
        public async Task GetHistoryYearlyAsync_HappyCase_ReturnsList()
        {
            var history = new List<CreditScoreHistory> { new() { Id = 1, Score = 500 } };
            _mockRepo.Setup(r => r.GetHistoryYearlyAsync("123")).ReturnsAsync(history);
            var result = await _service.GetHistoryYearlyAsync("123");
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetHistoryYearlyAsync_EmptyCnp_ThrowsArgumentException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _service.GetHistoryYearlyAsync(""));
        }

        [TestMethod]
        public async Task GetHistoryYearlyAsync_RepositoryThrows_ThrowsException()
        {
            _mockRepo.Setup(r => r.GetHistoryYearlyAsync("123")).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Common.Exceptions.HistoryServiceException>(async () => await _service.GetHistoryYearlyAsync("123"));
        }
    }
}