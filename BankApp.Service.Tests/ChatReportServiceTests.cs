using BankApi.Repositories;
using BankApi.Services;
using Common.Models;
using Common.Models.Social;
using Common.Services;
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
    public class ChatReportServiceTests
    {
        private Mock<IChatReportRepository> _mockChatReportRepo;
        private Mock<IUserRepository> _mockUserRepo;
        private Mock<ITipsService> _mockTipsService;
        private Mock<IMessagesService> _mockMessagesService;
        private Mock<IProfanityChecker> _mockProfanityChecker;
        private ChatReportService _service;

        [TestInitialize]
        public void Init()
        {
            _mockChatReportRepo = new Mock<IChatReportRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockTipsService = new Mock<ITipsService>();
            _mockMessagesService = new Mock<IMessagesService>();
            _mockProfanityChecker = new Mock<IProfanityChecker>();
            _service = new ChatReportService(
                _mockChatReportRepo.Object,
                _mockUserRepo.Object,
                _mockTipsService.Object,
                _mockMessagesService.Object,
                _mockProfanityChecker.Object);
        }

        [TestMethod]
        public async Task DoNotPunishUser_HappyCase_DeletesReport()
        {
            var report = new ChatReport
            {
                Id = 1,
                ReportedUserCnp = "123",
                SubmitterCnp = "456",
                Submitter = new User { CNP = "456" },
                ReportedUser = new User { CNP = "123" },
                MessageId = 1,
                Message = new Message { Id = 1, MessageContent = "test message" }
            };
            _mockChatReportRepo.Setup(r => r.DeleteChatReportAsync(report.Id)).Returns(Task.FromResult(true));
            await _service.DoNotPunishUser(report);
            _mockChatReportRepo.Verify(r => r.DeleteChatReportAsync(report.Id), Times.Once);
        }

        [TestMethod]
        public async Task PunishUser_NullReport_ThrowsArgumentNullException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await _service.PunishUser(null));
        }

        [TestMethod]
        public async Task PunishUser_EmptyReportedUserCnp_ThrowsArgumentException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _service.PunishUser(new ChatReport { ReportedUserCnp = "", SubmitterCnp = "456", Submitter = new User { CNP = "456" }, ReportedUser = new User { CNP = "" }, MessageId = 1, Message = new Message { Id = 1, MessageContent = "test message" } }));
        }

        [TestMethod]
        public async Task PunishUser_UserNotFound_ThrowsException()
        {
            var report = new ChatReport { ReportedUserCnp = "123", SubmitterCnp = "456", Submitter = new User { CNP = "456" }, ReportedUser = new User { CNP = "123" }, MessageId = 1, Message = new Message { Id = 1, MessageContent = "test message" } };
            _mockUserRepo.Setup(r => r.GetByCnpAsync("123")).ReturnsAsync((User)null);
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _service.PunishUser(report));
        }
        [TestMethod]
        public async Task PunishUser_HappyCase_PunishesUserAndDeletesReport()
        {
            var report = new ChatReport { Id = 1, ReportedUserCnp = "123", SubmitterCnp = "456", Submitter = new User { CNP = "456" }, ReportedUser = new User { CNP = "123" }, MessageId = 1, Message = new Message { Id = 1, MessageContent = "test message" } };
            var user = new User { Id = 2, NumberOfOffenses = 2, CreditScore = 500, GemBalance = 100, CNP = "123" };
            _mockUserRepo.Setup(r => r.GetByCnpAsync("123")).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(true);
            _mockChatReportRepo.Setup(r => r.UpdateScoreHistoryForUserAsync("123", It.IsAny<int>())).Returns(Task.FromResult(true));
            _mockChatReportRepo.Setup(r => r.DeleteChatReportAsync(1)).Returns(Task.FromResult(true));
            _mockTipsService.Setup(t => t.GiveTipToUserAsync("123")).Returns(Task.CompletedTask);
            _mockChatReportRepo.Setup(r => r.GetNumberOfGivenTipsForUserAsync("123")).ReturnsAsync(1);
            _mockChatReportRepo.Setup(r => r.UpdateActivityLogAsync("123", It.IsAny<int>())).Returns(Task.FromResult(true));
            await _service.PunishUser(report);
            _mockUserRepo.Verify(r => r.GetByCnpAsync("123"), Times.Once);
            _mockUserRepo.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.AtLeastOnce);
            _mockChatReportRepo.Verify(r => r.DeleteChatReportAsync(1), Times.Once);
        }

        [TestMethod]
        public async Task IsMessageOffensive_HappyCase_ReturnsFalse()
        {
            _mockProfanityChecker.Setup(p => p.IsMessageOffensive("hello")).ReturnsAsync(false);
            var result = await _service.IsMessageOffensive("hello");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task IsMessageOffensive_Offensive_ReturnsTrue()
        {
            _mockProfanityChecker.Setup(p => p.IsMessageOffensive("badword")).ReturnsAsync(true);
            var result = await _service.IsMessageOffensive("badword");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task IsMessageOffensive_Null_ThrowsException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await _service.IsMessageOffensive(null));
        }

        [TestMethod]
        public async Task GetAllChatReportsAsync_HappyCase_ReturnsList()
        {
            var reports = new List<ChatReport> { new() { Id = 1, ReportedUserCnp = "123", SubmitterCnp = "456", Submitter = new User { CNP = "456" }, ReportedUser = new User { CNP = "123" }, MessageId = 1, Message = new Message { Id = 1, MessageContent = "test message" } } };
            _mockChatReportRepo.Setup(r => r.GetAllChatReportsAsync()).ReturnsAsync(reports);
            var result = await _service.GetAllChatReportsAsync();
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetAllChatReportsAsync_Empty_ReturnsEmptyList()
        {
            _mockChatReportRepo.Setup(r => r.GetAllChatReportsAsync()).ReturnsAsync([]);
            var result = await _service.GetAllChatReportsAsync();
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetAllChatReportsAsync_RepositoryThrows_ThrowsException()
        {
            _mockChatReportRepo.Setup(r => r.GetAllChatReportsAsync()).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _service.GetAllChatReportsAsync());
        }
        [TestMethod]
        public async Task DeleteChatReportAsync_HappyCase_DeletesReport()
        {
            var report = new ChatReport { Id = 1, ReportedUserCnp = "123", SubmitterCnp = "456", Submitter = new User { CNP = "456" }, ReportedUser = new User { CNP = "123" }, MessageId = 1, Message = new Message { Id = 1, MessageContent = "test message" } };
            _mockChatReportRepo.Setup(r => r.DeleteChatReportAsync(report.Id)).Returns(Task.FromResult(true));
            await _service.DeleteChatReportAsync(report.Id);
            _mockChatReportRepo.Verify(r => r.DeleteChatReportAsync(report.Id), Times.Once);
        }
        [TestMethod]
        public async Task DeleteChatReportAsync_RepositoryThrows_ThrowsException()
        {
            var report = new ChatReport { Id = 1, ReportedUserCnp = "123", SubmitterCnp = "456", Submitter = new User { CNP = "456" }, ReportedUser = new User { CNP = "123" }, MessageId = 1, Message = new Message { Id = 1, MessageContent = "test message" } };
            _mockChatReportRepo.Setup(r => r.DeleteChatReportAsync(report.Id)).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _service.DeleteChatReportAsync(report.Id));
        }

        [TestMethod]
        public async Task UpdateScoreHistoryForUserAsync_HappyCase_UpdatesScore()
        {
            _mockChatReportRepo.Setup(r => r.UpdateScoreHistoryForUserAsync("123", It.IsAny<int>())).Returns(Task.FromResult(true));
            await _service.UpdateScoreHistoryForUserAsync(100, "123");
            _mockChatReportRepo.Verify(r => r.UpdateScoreHistoryForUserAsync("123", 100), Times.Once);
        }

        [TestMethod]
        public async Task UpdateScoreHistoryForUserAsync_RepositoryThrows_ThrowsException()
        {
            _mockChatReportRepo.Setup(r => r.UpdateScoreHistoryForUserAsync("123", It.IsAny<int>())).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _service.UpdateScoreHistoryForUserAsync(100, "123"));
        }
        [TestMethod]
        public async Task AddChatReportAsync_HappyCase_AddsReport()
        {
            var report = new ChatReport { Id = 1, ReportedUserCnp = "123", SubmitterCnp = "456", Submitter = new User { CNP = "456" }, ReportedUser = new User { CNP = "123" }, MessageId = 1, Message = new Message { Id = 1, MessageContent = "test message" } };
            _mockChatReportRepo.Setup(r => r.AddChatReportAsync(report)).Returns(Task.FromResult(true));
            await _service.AddChatReportAsync(report);
            _mockChatReportRepo.Verify(r => r.AddChatReportAsync(report), Times.Once);
        }
        [TestMethod]
        public async Task AddChatReportAsync_RepositoryThrows_ThrowsException()
        {
            var report = new ChatReport { Id = 1, ReportedUserCnp = "123", SubmitterCnp = "456", Submitter = new User { CNP = "456" }, ReportedUser = new User { CNP = "123" }, MessageId = 1, Message = new Message { Id = 1, MessageContent = "test message" } };
            _mockChatReportRepo.Setup(r => r.AddChatReportAsync(report)).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _service.AddChatReportAsync(report));
        }
        [TestMethod]
        public async Task GetChatReportByIdAsync_HappyCase_ReturnsReport()
        {
            var report = new ChatReport { Id = 1, ReportedUserCnp = "123", SubmitterCnp = "456", Submitter = new User { CNP = "456" }, ReportedUser = new User { CNP = "123" }, MessageId = 1, Message = new Message { Id = 1, MessageContent = "test message" } };
            _mockChatReportRepo.Setup(r => r.GetChatReportByIdAsync(1)).ReturnsAsync(report);
            var result = await _service.GetChatReportByIdAsync(1);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
        }

        [TestMethod]
        public async Task GetChatReportByIdAsync_NotFound_ReturnsNull()
        {
            _mockChatReportRepo.Setup(r => r.GetChatReportByIdAsync(1)).ReturnsAsync((ChatReport)null);
            var result = await _service.GetChatReportByIdAsync(1);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetChatReportByIdAsync_RepositoryThrows_ThrowsException()
        {
            _mockChatReportRepo.Setup(r => r.GetChatReportByIdAsync(1)).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _service.GetChatReportByIdAsync(1));
        }

        [TestMethod]
        public async Task GetNumberOfGivenTipsForUserAsync_HappyCase_ReturnsNumber()
        {
            _mockChatReportRepo.Setup(r => r.GetNumberOfGivenTipsForUserAsync("123")).ReturnsAsync(5);
            var result = await _service.GetNumberOfGivenTipsForUserAsync("123");
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public async Task GetNumberOfGivenTipsForUserAsync_RepositoryThrows_ThrowsException()
        {
            _mockChatReportRepo.Setup(r => r.GetNumberOfGivenTipsForUserAsync("123")).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _service.GetNumberOfGivenTipsForUserAsync("123"));
        }

        [TestMethod]
        public async Task UpdateActivityLogAsync_HappyCase_UpdatesLog()
        {
            _mockChatReportRepo.Setup(r => r.UpdateActivityLogAsync("123", It.IsAny<int>())).Returns(Task.FromResult(true));
            await _service.UpdateActivityLogAsync(10, "123");
            _mockChatReportRepo.Verify(r => r.UpdateActivityLogAsync("123", 10), Times.Once);
        }

        [TestMethod]
        public async Task UpdateActivityLogAsync_RepositoryThrows_ThrowsException()
        {
            _mockChatReportRepo.Setup(r => r.UpdateActivityLogAsync("123", It.IsAny<int>())).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _service.UpdateActivityLogAsync(10, "123"));
        }
    }
}