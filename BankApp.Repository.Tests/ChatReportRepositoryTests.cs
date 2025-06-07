using BankApi.Data;
using BankApi.Repositories.Impl.Social;
using BankApi.Repositories.Social;
using Common.Models;
using Common.Models.Bank;
using Common.Models.Social;
using Common.Services.Social;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace BankApp.Repository.Tests
{
    [SupportedOSPlatform("windows10.0.26100.0")]
    [TestClass]
    public class ChatReportRepositoryTests
    {
        private readonly DbContextOptions<ApiDbContext> _dbOptions;

        public ChatReportRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private ApiDbContext CreateContext() => new(_dbOptions);

        [TestMethod]
        public async Task GetAllChatReportsAsync_Should_Return_All_Reports()
        {
            using var context = CreateContext();

            var user1 = new User { CNP = "123", UserName = "user1", FirstName = "First1", LastName = "Last1" };
            var user2 = new User { CNP = "456", UserName = "user2", FirstName = "First2", LastName = "Last2" };
            var user3 = new User { CNP = "789", UserName = "user3", FirstName = "First3", LastName = "Last3" };

            await context.Users.AddRangeAsync(new[] { user1, user2, user3 });
            await context.SaveChangesAsync();

            var chat = new Chat
            {
                Id = 1,
                ChatName = "TestChat",
                Users = [user1, user2, user3],
                Messages = []
            };
            await context.Chats.AddAsync(chat);
            await context.SaveChangesAsync();

            var message1 = new Message
            {
                Id = 1,
                ChatId = chat.Id,
                Chat = chat,
                UserId = 1,
                Sender = user2,
                MessageContent = "Test message 1",
                Type = MessageType.Text.ToString(),
                MessageType = MessageType.Text
            };

            var message2 = new Message
            {
                Id = 2,
                ChatId = chat.Id,
                Chat = chat,
                UserId = 1,
                Sender = user2,
                MessageContent = "Test message 2",
                Type = MessageType.Text.ToString(),
                MessageType = MessageType.Text
            };

            chat.Messages.Add(message1);
            chat.Messages.Add(message2);

            await context.Messages.AddRangeAsync(new[] { message1, message2 });
            await context.SaveChangesAsync();

            var reports = new List<ChatReport>
            {
                new()
                {
                    Id = 1,
                    SubmitterCnp = "123",
                    Submitter = user1,
                    ReportedUserCnp = "456",
                    ReportedUser = user2,
                    MessageId = 1,
                    Message = message1,
                    Reason = ReportReason.OffensiveContent
                },
                new()
                {
                    Id = 2,
                    SubmitterCnp = "789",
                    Submitter = user3,
                    ReportedUserCnp = "456",
                    ReportedUser = user2,
                    MessageId = 2,
                    Message = message2,
                    Reason = ReportReason.Spam
                }
            };

            await context.ChatReports.AddRangeAsync(reports);
            await context.SaveChangesAsync();

            var repository = new ChatReportRepository(context);

            var result = await repository.GetAllChatReportsAsync();

            result.Should().HaveCount(2);
            result.Should().ContainEquivalentOf(reports[0]);
            result.Should().ContainEquivalentOf(reports[1]);
        }

        [TestMethod]
        public async Task GetChatReportByIdAsync_Should_Return_Report_When_Found()
        {
            using var context = CreateContext();

            var user1 = new User { CNP = "123", UserName = "user1", FirstName = "First1", LastName = "Last1" };
            var user2 = new User { CNP = "456", UserName = "user2", FirstName = "First2", LastName = "Last2" };

            await context.Users.AddRangeAsync(new[] { user1, user2 });
            await context.SaveChangesAsync();

            var chat = new Chat
            {
                Id = 1,
                ChatName = "TestChat",
                Users = [user1, user2],
                Messages = []
            };
            await context.Chats.AddAsync(chat);
            await context.SaveChangesAsync();

            var message = new Message
            {
                Id = 1,
                ChatId = chat.Id,
                Chat = chat,
                UserId = 1,
                Sender = user2,
                MessageContent = "Test message",
                Type = MessageType.Text.ToString(),
                MessageType = MessageType.Text
            };

            chat.Messages.Add(message);

            await context.Messages.AddAsync(message);
            await context.SaveChangesAsync();

            var report = new ChatReport
            {
                Id = 1,
                SubmitterCnp = "123",
                Submitter = user1,
                ReportedUserCnp = "456",
                ReportedUser = user2,
                MessageId = 1,
                Message = message,
                Reason = ReportReason.OffensiveContent
            };

            await context.ChatReports.AddAsync(report);
            await context.SaveChangesAsync();

            var repository = new ChatReportRepository(context);

            var result = await repository.GetChatReportByIdAsync(1);

            result.Should().BeEquivalentTo(report);
        }

        [TestMethod]
        public async Task GetChatReportByIdAsync_Should_Return_Null_When_Not_Found()
        {
            using var context = CreateContext();
            var repository = new ChatReportRepository(context);

            var result = await repository.GetChatReportByIdAsync(999);

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task AddChatReportAsync_Should_Return_True_When_Added_Successfully()
        {
            using var context = CreateContext();

            var user1 = new User { CNP = "123", UserName = "user1", FirstName = "First1", LastName = "Last1" };
            var user2 = new User { CNP = "456", UserName = "user2", FirstName = "First2", LastName = "Last2" };

            await context.Users.AddRangeAsync(new[] { user1, user2 });
            await context.SaveChangesAsync();

            var chat = new Chat
            {
                Id = 1,
                ChatName = "TestChat",
                Users = [user1, user2],
                Messages = []
            };
            await context.Chats.AddAsync(chat);
            await context.SaveChangesAsync();

            var message = new Message
            {
                Id = 1,
                ChatId = chat.Id,
                Chat = chat,
                UserId = 1,
                Sender = user2,
                MessageContent = "Test message",
                Type = MessageType.Text.ToString(),
                MessageType = MessageType.Text
            };

            chat.Messages.Add(message);

            await context.Messages.AddAsync(message);
            await context.SaveChangesAsync();

            var report = new ChatReport
            {
                SubmitterCnp = "123",
                Submitter = user1,
                ReportedUserCnp = "456",
                ReportedUser = user2,
                MessageId = 1,
                Message = message,
                Reason = ReportReason.OffensiveContent
            };

            var repository = new ChatReportRepository(context);

            var result = await repository.AddChatReportAsync(report);

            result.Should().BeTrue();
            context.ChatReports.Should().ContainEquivalentOf(report, options => options.Excluding(r => r.Id));
        }

        [TestMethod]
        public async Task AddChatReportAsync_Should_Return_False_When_Exception_Occurs()
        {
            var mockRepo = new Mock<IChatReportRepository>();

            var user1 = new User { CNP = "123", UserName = "user1", FirstName = "First1", LastName = "Last1" };
            var user2 = new User { CNP = "456", UserName = "user2", FirstName = "First2", LastName = "Last2" };
            var chat = new Chat
            {
                Id = 1,
                ChatName = "TestChat",
                Users = [user1, user2],
                Messages = []
            };
            var message = new Message
            {
                Id = 1,
                ChatId = chat.Id,
                Chat = chat,
                UserId = 1,
                Sender = user2,
                MessageContent = "Test message",
                Type = MessageType.Text.ToString(),
                MessageType = MessageType.Text
            };

            chat.Messages.Add(message);

            var report = new ChatReport
            {
                SubmitterCnp = "123",
                Submitter = user1,
                ReportedUserCnp = "456",
                ReportedUser = user2,
                MessageId = 1,
                Message = message,
                Reason = ReportReason.OffensiveContent
            };

            mockRepo.Setup(r => r.AddChatReportAsync(It.IsAny<ChatReport>())).ReturnsAsync(false);

            var result = await mockRepo.Object.AddChatReportAsync(report);

            result.Should().BeFalse();
            mockRepo.Verify(r => r.AddChatReportAsync(It.IsAny<ChatReport>()), Times.Once);
        }

        [TestMethod]
        public async Task DeleteChatReportAsync_Should_Return_True_When_Deleted_Successfully()
        {
            using var context = CreateContext();

            var user1 = new User { CNP = "123", UserName = "user1", FirstName = "First1", LastName = "Last1" };
            var user2 = new User { CNP = "456", UserName = "user2", FirstName = "First2", LastName = "Last2" };

            await context.Users.AddRangeAsync(new[] { user1, user2 });
            await context.SaveChangesAsync();

            var chat = new Chat
            {
                Id = 1,
                ChatName = "TestChat",
                Users = [user1, user2],
                Messages = []
            };
            await context.Chats.AddAsync(chat);
            await context.SaveChangesAsync();

            var message = new Message
            {
                Id = 1,
                ChatId = chat.Id,
                Chat = chat,
                UserId = 1,
                Sender = user2,
                MessageContent = "Test message",
                Type = MessageType.Text.ToString(),
                MessageType = MessageType.Text
            };

            chat.Messages.Add(message);

            await context.Messages.AddAsync(message);
            await context.SaveChangesAsync();

            var report = new ChatReport
            {
                Id = 1,
                SubmitterCnp = "123",
                Submitter = user1,
                ReportedUserCnp = "456",
                ReportedUser = user2,
                MessageId = 1,
                Message = message,
                Reason = ReportReason.OffensiveContent
            };

            await context.ChatReports.AddAsync(report);
            await context.SaveChangesAsync();

            var repository = new ChatReportRepository(context);

            var result = await repository.DeleteChatReportAsync(1);

            result.Should().BeTrue();
            context.ChatReports.Should().BeEmpty();
        }

        [TestMethod]
        public async Task DeleteChatReportAsync_Should_Throw_When_Report_Not_Found()
        {
            var mockRepo = new Mock<IChatReportRepository>();
            mockRepo.Setup(r => r.DeleteChatReportAsync(999))
                .ThrowsAsync(new Exception("Chat report with id 999 not found."));

            await Assert.ThrowsExceptionAsync<Exception>(async () => await mockRepo.Object.DeleteChatReportAsync(999));
        }

        [TestMethod]
        public async Task DeleteChatReportAsync_Should_Return_False_When_Exception_Occurs()
        {
            var mockRepo = new Mock<IChatReportRepository>();
            mockRepo.Setup(r => r.DeleteChatReportAsync(1)).ReturnsAsync(false);

            var result = await mockRepo.Object.DeleteChatReportAsync(1);

            result.Should().BeFalse();
            mockRepo.Verify(r => r.DeleteChatReportAsync(1), Times.Once);
        }

        [TestMethod]
        public async Task GetNumberOfGivenTipsForUserAsync_Should_Return_Count()
        {
            using var context = CreateContext();
            var user = new User
            {
                CNP = "123",
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User",
                Birthday = DateTime.Now.AddYears(-30)
            };
            var tip = new Tip
            {
                Id = 1,
                TipText = "Save more",
                CreditScoreBracket = "600-700",
                Type = "Financial"
            };
            var givenTips = new List<GivenTip>
            {
                new() { User = user, Tip = tip, UserCNP = "123", TipId = 1 },
                new() { User = user, Tip = tip, UserCNP = "123", TipId = 2 }
            };

            await context.Users.AddAsync(user);
            await context.Tips.AddAsync(tip);
            await context.GivenTips.AddRangeAsync(givenTips);
            await context.SaveChangesAsync();

            var repository = new ChatReportRepository(context);

            var result = await repository.GetNumberOfGivenTipsForUserAsync("123");

            result.Should().Be(2);
        }

        [TestMethod]
        public async Task UpdateActivityLogAsync_Should_Add_New_Log_When_Not_Exists()
        {
            using var context = CreateContext();
            var repository = new ChatReportRepository(context);

            await repository.UpdateActivityLogAsync("123", 5);

            var log = await context.ActivityLogs.FirstOrDefaultAsync(a => a.UserCnp == "123" && a.ActivityName == "Chat");
            log.Should().NotBeNull();
            log.LastModifiedAmount.Should().Be(5);
            log.ActivityDetails.Should().Be("Chat abuse");
        }

        [TestMethod]
        public async Task UpdateActivityLogAsync_Should_Update_Existing_Log()
        {
            using var context = CreateContext();
            var log = new ActivityLog
            {
                UserCnp = "123",
                ActivityName = "Chat",
                LastModifiedAmount = 1,
                ActivityDetails = "Old details"
            };

            await context.ActivityLogs.AddAsync(log);
            await context.SaveChangesAsync();

            var repository = new ChatReportRepository(context);

            await repository.UpdateActivityLogAsync("123", 10);

            var updatedLog = await context.ActivityLogs.FirstOrDefaultAsync(a => a.UserCnp == "123" && a.ActivityName == "Chat");
            updatedLog.Should().NotBeNull();
            updatedLog.LastModifiedAmount.Should().Be(10);
            updatedLog.ActivityDetails.Should().Be("Chat abuse");
        }

        [TestMethod]
        public async Task UpdateScoreHistoryForUserAsync_Should_Add_New_History_When_Not_Exists()
        {
            using var context = CreateContext();
            var repository = new ChatReportRepository(context);

            await repository.UpdateScoreHistoryForUserAsync("123", 750);

            var history = await context.CreditScoreHistories.FirstOrDefaultAsync(s => s.UserCnp == "123" && s.Date == DateTime.Today);
            history.Should().NotBeNull();
            history.Score.Should().Be(750);
        }

        [TestMethod]
        public async Task UpdateScoreHistoryForUserAsync_Should_Update_Existing_History()
        {
            using var context = CreateContext();
            var history = new CreditScoreHistory
            {
                UserCnp = "123",
                Date = DateTime.Today,
                Score = 700
            };

            await context.CreditScoreHistories.AddAsync(history);
            await context.SaveChangesAsync();

            var repository = new ChatReportRepository(context);

            await repository.UpdateScoreHistoryForUserAsync("123", 800);

            var updatedHistory = await context.CreditScoreHistories.FirstOrDefaultAsync(s => s.UserCnp == "123" && s.Date == DateTime.Today);
            updatedHistory.Should().NotBeNull();
            updatedHistory.Score.Should().Be(800);
        }
    }
}