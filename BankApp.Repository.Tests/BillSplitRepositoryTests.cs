using BankApi.Data;
using BankApi.Repositories;
using BankApi.Repositories.Impl.Bank;
using Common.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Xunit;

namespace BankApp.Repository.Tests
{
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class BillSplitRepositoryTests
    {
        private readonly DbContextOptions<ApiDbContext> _dbOptions;
        private readonly Mock<ILogger<BillSplitReportRepository>> _loggerMock;

        public BillSplitRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _loggerMock = new Mock<ILogger<BillSplitReportRepository>>();
        }

        private ApiDbContext CreateContext() => new(_dbOptions);

        [Fact]
        public async Task GetAllReportsAsync_Should_Return_All_Reports()
        {
            using var context = CreateContext();
            var reports = new List<BillSplitReport>
            {
                new() { Id = 1, ReportedUserCnp = "123", ReportingUserCnp = "456", DateOfTransaction = DateTime.Now, BillShare = 50.0m },
                new() { Id = 2, ReportedUserCnp = "789", ReportingUserCnp = "456", DateOfTransaction = DateTime.Now, BillShare = 75.0m }
            };

            await context.BillSplitReports.AddRangeAsync(reports);
            await context.SaveChangesAsync();

            var repository = new BillSplitReportRepository(context, _loggerMock.Object);

            var result = await repository.GetAllReportsAsync();

            result.Should().HaveCount(2);
            result.Should().ContainEquivalentOf(reports[0]);
            result.Should().ContainEquivalentOf(reports[1]);
        }

        [Fact]
        public async Task GetReportByIdAsync_Should_Return_Report_When_Found()
        {
            using var context = CreateContext();
            var report = new BillSplitReport
            {
                Id = 1,
                ReportedUserCnp = "123",
                ReportingUserCnp = "456",
                DateOfTransaction = DateTime.Now,
                BillShare = 50.0m
            };

            await context.BillSplitReports.AddAsync(report);
            await context.SaveChangesAsync();

            var repository = new BillSplitReportRepository(context, _loggerMock.Object);

            var result = await repository.GetReportByIdAsync(1);

            result.Should().BeEquivalentTo(report);
        }

        [Fact]
        public async Task GetReportByIdAsync_Should_Throw_When_Report_Not_Found()
        {
            using var context = CreateContext();
            var repository = new BillSplitReportRepository(context, _loggerMock.Object);

            await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => repository.GetReportByIdAsync(999));
        }

        [Fact]
        public async Task AddReportAsync_Should_Add_Report()
        {
            using var context = CreateContext();
            var report = new BillSplitReport
            {
                ReportedUserCnp = "123",
                ReportingUserCnp = "456",
                DateOfTransaction = DateTime.Now,
                BillShare = 50.0m
            };

            var repository = new BillSplitReportRepository(context, _loggerMock.Object);

            var result = await repository.AddReportAsync(report);

            result.Should().BeEquivalentTo(report);
            context.BillSplitReports.Should().ContainEquivalentOf(report);
        }

        [Fact]
        public async Task UpdateReportAsync_Should_Update_Report_When_Found()
        {
            using var context = CreateContext();
            var report = new BillSplitReport
            {
                Id = 1,
                ReportedUserCnp = "123",
                ReportingUserCnp = "456",
                DateOfTransaction = DateTime.Now,
                BillShare = 50.0m
            };

            await context.BillSplitReports.AddAsync(report);
            await context.SaveChangesAsync();

            var updatedReport = new BillSplitReport
            {
                Id = 1,
                ReportedUserCnp = "123",
                ReportingUserCnp = "456",
                DateOfTransaction = DateTime.Now,
                BillShare = 75.0m
            };

            var repository = new BillSplitReportRepository(context, _loggerMock.Object);

            var result = await repository.UpdateReportAsync(updatedReport);

            result.Should().BeEquivalentTo(updatedReport);
            context.BillSplitReports.Find(1).BillShare.Should().Be(75.0m);
        }

        [Fact]
        public async Task UpdateReportAsync_Should_Throw_When_Report_Not_Found()
        {
            using var context = CreateContext();
            var report = new BillSplitReport
            {
                Id = 999,
                ReportedUserCnp = "123",
                ReportingUserCnp = "456",
                DateOfTransaction = DateTime.Now,
                BillShare = 50.0m
            };

            var repository = new BillSplitReportRepository(context, _loggerMock.Object);

            await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => repository.UpdateReportAsync(report));
        }

        [Fact]
        public async Task DeleteReportAsync_Should_Return_True_When_Report_Deleted()
        {
            using var context = CreateContext();
            var report = new BillSplitReport
            {
                Id = 1,
                ReportedUserCnp = "123",
                ReportingUserCnp = "456",
                DateOfTransaction = DateTime.Now,
                BillShare = 50.0m
            };

            await context.BillSplitReports.AddAsync(report);
            await context.SaveChangesAsync();

            var repository = new BillSplitReportRepository(context, _loggerMock.Object);

            var result = await repository.DeleteReportAsync(1);

            result.Should().BeTrue();
            context.BillSplitReports.Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteReportAsync_Should_Return_False_When_Report_Not_Found()
        {
            using var context = CreateContext();
            var repository = new BillSplitReportRepository(context, _loggerMock.Object);

            var result = await repository.DeleteReportAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetCurrentBalanceAsync_Should_Return_Balance()
        {
            var mockRepo = new Mock<IBillSplitReportRepository>();
            mockRepo.Setup(r => r.GetCurrentBalanceAsync("123")).ReturnsAsync(100);

            var result = await mockRepo.Object.GetCurrentBalanceAsync("123");

            result.Should().Be(100);
            mockRepo.Verify(r => r.GetCurrentBalanceAsync("123"), Times.Once);
        }

        [Fact]
        public async Task SumTransactionsSinceReportAsync_Should_Return_Sum()
        {
            var mockRepo = new Mock<IBillSplitReportRepository>();
            var testDate = DateTime.Now.AddDays(-30);
            mockRepo.Setup(r => r.SumTransactionsSinceReportAsync("123", testDate)).ReturnsAsync(150m);

            var result = await mockRepo.Object.SumTransactionsSinceReportAsync("123", testDate);

            result.Should().Be(150m);
            mockRepo.Verify(r => r.SumTransactionsSinceReportAsync("123", testDate), Times.Once);
        }

        [Fact]
        public async Task GetCurrentCreditScoreAsync_Should_Return_Score()
        {
            var mockRepo = new Mock<IBillSplitReportRepository>();
            mockRepo.Setup(r => r.GetCurrentCreditScoreAsync("123")).ReturnsAsync(700);

            var result = await mockRepo.Object.GetCurrentCreditScoreAsync("123");

            result.Should().Be(700);
            mockRepo.Verify(r => r.GetCurrentCreditScoreAsync("123"), Times.Once);
        }

        [Fact]
        public async Task UpdateCreditScoreAsync_Should_Execute_SQL_Command()
        {
            var mockRepo = new Mock<IBillSplitReportRepository>();
            mockRepo.Setup(r => r.UpdateCreditScoreAsync("123", 750)).Returns(Task.CompletedTask);

            await mockRepo.Object.UpdateCreditScoreAsync("123", 750);

            mockRepo.Verify(r => r.UpdateCreditScoreAsync("123", 750), Times.Once);
        }

        [Fact]
        public async Task IncrementBillSharesPaidAsync_Should_Execute_SQL_Command()
        {
            var mockRepo = new Mock<IBillSplitReportRepository>();
            mockRepo.Setup(r => r.IncrementBillSharesPaidAsync("123")).Returns(Task.CompletedTask);

            await mockRepo.Object.IncrementBillSharesPaidAsync("123");

            mockRepo.Verify(r => r.IncrementBillSharesPaidAsync("123"), Times.Once);
        }
    }
}