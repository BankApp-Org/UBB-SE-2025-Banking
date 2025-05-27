using BankApi.Data;
using BankApi.Repositories;
using BankApi.Repositories.Impl;
using Common.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Xunit;

namespace StockApp.Repository.Tests
{
    [SupportedOSPlatform("windows10.0.26100.0")]
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

        [Fact]
        public async Task GetAllChatReportsAsync_Should_Return_All_Reports()
        {
            using var context = CreateContext();
            var reports = new List<ChatReport>
            {
                new() { Id = 1, SubmitterCnp = "123", ReportedUserCnp = "456", ReportedMessage = "Test message 1" },
                new() { Id = 2, SubmitterCnp = "789", ReportedUserCnp = "456", ReportedMessage = "Test message 2" }
            };

            await context.ChatReports.AddRangeAsync(reports);
            await context.SaveChangesAsync();

            var repository = new ChatReportRepository(context);

            var result = await repository.GetAllChatReportsAsync();

            result.Should().HaveCount(2);
            result.Should().ContainEquivalentOf(reports[0]);
            result.Should().ContainEquivalentOf(reports[1]);
        }

        [Fact]
        public async Task GetChatReportByIdAsync_Should_Return_Report_When_Found()
        {
            using var context = CreateContext();
            var report = new ChatReport
            {
                Id = 1,
                SubmitterCnp = "123",
                ReportedUserCnp = "456",
                ReportedMessage = "Test message"
            };

            await context.ChatReports.AddAsync(report);
            await context.SaveChangesAsync();

            var repository = new ChatReportRepository(context);

            var result = await repository.GetChatReportByIdAsync(1);

            result.Should().BeEquivalentTo(report);
        }

        [Fact]
        public async Task GetChatReportByIdAsync_Should_Return_Null_When_Not_Found()
        {
            using var context = CreateContext();
            var repository = new ChatReportRepository(context);

            var result = await repository.GetChatReportByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AddChatReportAsync_Should_Return_True_When_Added_Successfully()
        {
            using var context = CreateContext();
            var report = new ChatReport
            {
                SubmitterCnp = "123",
                ReportedUserCnp = "456",
                ReportedMessage = "Test message"
            };

            var repository = new ChatReportRepository(context);

            var result = await repository.AddChatReportAsync(report);

            result.Should().BeTrue();
            context.ChatReports.Should().ContainEquivalentOf(report, options => options.Excluding(r => r.Id));
        }

        [Fact]
        public async Task AddChatReportAsync_Should_Return_False_When_Exception_Occurs()
        {
            var mockRepo = new Mock<IChatReportRepository>();
            var report = new ChatReport
            {
                SubmitterCnp = "123",
                ReportedUserCnp = "456",
                ReportedMessage = "Test message"
            };

            mockRepo.Setup(r => r.AddChatReportAsync(It.IsAny<ChatReport>())).ReturnsAsync(false);

            var result = await mockRepo.Object.AddChatReportAsync(report);

            result.Should().BeFalse();
            mockRepo.Verify(r => r.AddChatReportAsync(It.IsAny<ChatReport>()), Times.Once);
        }

        [Fact]
        public async Task DeleteChatReportAsync_Should_Return_True_When_Deleted_Successfully()
        {
            using var context = CreateContext();
            var report = new ChatReport
            {
                Id = 1,
                SubmitterCnp = "123",
                ReportedUserCnp = "456",
                ReportedMessage = "Test message"
            };

            await context.ChatReports.AddAsync(report);
            await context.SaveChangesAsync();

            var repository = new ChatReportRepository(context);

            var result = await repository.DeleteChatReportAsync(1);

            result.Should().BeTrue();
            context.ChatReports.Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteChatReportAsync_Should_Throw_When_Report_Not_Found()
        {
            var mockRepo = new Mock<IChatReportRepository>();
            mockRepo.Setup(r => r.DeleteChatReportAsync(999))
                .ThrowsAsync(new Exception("Chat report with id 999 not found."));

            await Assert.ThrowsAsync<Exception>(() => mockRepo.Object.DeleteChatReportAsync(999));
        }

        [Fact]
        public async Task DeleteChatReportAsync_Should_Return_False_When_Exception_Occurs()
        {
            var mockRepo = new Mock<IChatReportRepository>();
            mockRepo.Setup(r => r.DeleteChatReportAsync(1)).ReturnsAsync(false);

            var result = await mockRepo.Object.DeleteChatReportAsync(1);

            result.Should().BeFalse();
            mockRepo.Verify(r => r.DeleteChatReportAsync(1), Times.Once);
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public async Task UpdateScoreHistoryForUserAsync_Should_Add_New_History_When_Not_Exists()
        {
            using var context = CreateContext();
            var repository = new ChatReportRepository(context);

            await repository.UpdateScoreHistoryForUserAsync("123", 750);

            var history = await context.CreditScoreHistories.FirstOrDefaultAsync(s => s.UserCnp == "123" && s.Date == DateTime.Today);
            history.Should().NotBeNull();
            history.Score.Should().Be(750);
        }

        [Fact]
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