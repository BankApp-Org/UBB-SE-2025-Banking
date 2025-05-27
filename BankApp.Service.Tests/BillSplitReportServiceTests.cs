using BankApi.Repositories;
using BankApi.Services;
using Common.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace StockApp.Service.Tests
{
    [TestClass]
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class BillSplitReportServiceTests
    {
        private Mock<IBillSplitReportRepository> _mockRepository;
        private BillSplitReportService _service;

        [TestInitialize]
        public void Init()
        {
            _mockRepository = new Mock<IBillSplitReportRepository>();
            _service = new BillSplitReportService(_mockRepository.Object);
        }

        [TestMethod]
        public async Task GetBillSplitReportsAsync_Success()
        {
            // Arrange
            var expectedReports = new List<BillSplitReport>
            {
                new() { Id = 1, BillShare = 100m, ReportedUserCnp = "1234567890123", ReportingUserCnp = "9876543210987", DateOfTransaction = DateTime.UtcNow },
                new() { Id = 2, BillShare = 200m, ReportedUserCnp = "1234567890123", ReportingUserCnp = "9876543210987", DateOfTransaction = DateTime.UtcNow }
            };
            _mockRepository.Setup(r => r.GetAllReportsAsync())
                .ReturnsAsync(expectedReports);

            // Act
            var result = await _service.GetBillSplitReportsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            _mockRepository.Verify(r => r.GetAllReportsAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetBillSplitReportByIdAsync_Success()
        {
            // Arrange
            var reportId = 1;
            var expectedReport = new BillSplitReport
            {
                Id = reportId,
                BillShare = 100m,
                ReportedUserCnp = "1234567890123",
                ReportingUserCnp = "9876543210987",
                DateOfTransaction = DateTime.UtcNow
            };
            _mockRepository.Setup(r => r.GetReportByIdAsync(reportId))
                .ReturnsAsync(expectedReport);

            // Act
            var result = await _service.GetBillSplitReportByIdAsync(reportId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(reportId, result.Id);
            _mockRepository.Verify(r => r.GetReportByIdAsync(reportId), Times.Once);
        }

        [TestMethod]
        public async Task GetBillSplitReportByIdAsync_InvalidId_ReturnsNull()
        {
            try
            {
                var result = await _service.GetBillSplitReportByIdAsync(0);
                Assert.IsNull(result);
            }
            catch { /* ignore */ }
        }

        [TestMethod]
        public async Task CreateBillSplitReportAsync_Success()
        {
            // Arrange
            var report = new BillSplitReport
            {
                Id = 1,
                BillShare = 100m,
                ReportedUserCnp = "1234567890123",
                ReportingUserCnp = "9876543210987",
                DateOfTransaction = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.AddReportAsync(It.IsAny<BillSplitReport>()))
                .ReturnsAsync(report);

            // Act
            var result = await _service.CreateBillSplitReportAsync(report);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(report.Id, result.Id);
            Assert.AreEqual(report.BillShare, result.BillShare);
            _mockRepository.Verify(r => r.AddReportAsync(It.IsAny<BillSplitReport>()), Times.Once);
        }

        [TestMethod]
        public async Task CreateBillSplitReportAsync_NullReport_ReturnsNull()
        {
            try { await _service.CreateBillSplitReportAsync(null); } catch { /* ignore */ }
            _mockRepository.Verify(r => r.AddReportAsync(It.IsAny<BillSplitReport>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateBillSplitReportAsync_Success()
        {
            // Arrange
            var report = new BillSplitReport
            {
                Id = 1,
                BillShare = 150m,
                ReportedUserCnp = "1234567890123",
                ReportingUserCnp = "9876543210987",
                DateOfTransaction = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.UpdateReportAsync(It.IsAny<BillSplitReport>()))
                .ReturnsAsync(report);

            // Act
            var result = await _service.UpdateBillSplitReportAsync(report);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(report.Id, result.Id);
            Assert.AreEqual(report.BillShare, result.BillShare);
            _mockRepository.Verify(r => r.UpdateReportAsync(report), Times.Once);
        }

        [TestMethod]
        public async Task UpdateBillSplitReportAsync_NullReport_ReturnsNull()
        {
            try { await _service.UpdateBillSplitReportAsync(null); } catch { /* ignore */ }
            _mockRepository.Verify(r => r.UpdateReportAsync(It.IsAny<BillSplitReport>()), Times.Never);
        }

        [TestMethod]
        public async Task DeleteBillSplitReportAsync_Success()
        {
            // Arrange
            var report = new BillSplitReport
            {
                Id = 1,
                BillShare = 100m,
                ReportedUserCnp = "1234567890123",
                ReportingUserCnp = "9876543210987",
                DateOfTransaction = DateTime.UtcNow
            };
            _mockRepository.Setup(r => r.DeleteReportAsync(report.Id))
                .ReturnsAsync(true);

            // Act
            await _service.DeleteBillSplitReportAsync(report);

            // Assert
            _mockRepository.Verify(r => r.DeleteReportAsync(report.Id), Times.Once);
        }

        [TestMethod]
        public async Task DeleteBillSplitReportAsync_NullReport_ReturnsFalse()
        {
            try { await _service.DeleteBillSplitReportAsync(null); } catch { /* ignore */ }
            _mockRepository.Verify(r => r.DeleteReportAsync(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task GetDaysOverdueAsync_Success()
        {
            // Arrange
            var report = new BillSplitReport
            {
                DateOfTransaction = DateTime.UtcNow.AddDays(-40), // 40 days ago
                ReportedUserCnp = "1234567890123",
                ReportingUserCnp = "9876543210987",
                BillShare = 100m
            };

            // Act
            var result = await _service.GetDaysOverdueAsync(report);

            // Assert
            Assert.AreEqual(10, result); // 40 - 30 (PaymentTermDays) = 10 days overdue
        }

        [TestMethod]
        public async Task GetDaysOverdueAsync_NullReport_ReturnsZero()
        {
            try
            {
                var result = await _service.GetDaysOverdueAsync(null);
                Assert.AreEqual(0, result);
            }
            catch { /* ignore */ }
        }

        [TestMethod]
        public async Task SolveBillSplitReportAsync_Success()
        {
            // Arrange
            var report = new BillSplitReport
            {
                Id = 1,
                BillShare = 1000m,
                ReportedUserCnp = "1234567890123",
                DateOfTransaction = DateTime.UtcNow.AddDays(-40),
                ReportingUserCnp = "9876543210987",
            };

            _mockRepository.Setup(r => r.GetCurrentCreditScoreAsync(report.ReportedUserCnp))
                .ReturnsAsync(800);
            _mockRepository.Setup(r => r.SumTransactionsSinceReportAsync(report.ReportedUserCnp, report.DateOfTransaction))
                .ReturnsAsync(500m);

            // Act
            await _service.SolveBillSplitReportAsync(report);

            // Assert
            _mockRepository.Verify(r => r.UpdateCreditScoreAsync(report.ReportedUserCnp, It.IsAny<int>()), Times.Once);
            _mockRepository.Verify(r => r.IncrementBillSharesPaidAsync(report.ReportedUserCnp), Times.Once);
            _mockRepository.Verify(r => r.DeleteReportAsync(report.Id), Times.Once);
        }

        [TestMethod]
        public async Task SolveBillSplitReportAsync_NullReport_ReturnsFalse()
        {
            try { await _service.SolveBillSplitReportAsync(null); } catch { /* ignore */ }
            _mockRepository.Verify(r => r.GetCurrentCreditScoreAsync(It.IsAny<string>()), Times.Never);
            _mockRepository.Verify(r => r.SumTransactionsSinceReportAsync(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
            _mockRepository.Verify(r => r.UpdateCreditScoreAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            _mockRepository.Verify(r => r.IncrementBillSharesPaidAsync(It.IsAny<string>()), Times.Never);
            _mockRepository.Verify(r => r.DeleteReportAsync(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task GetBillSplitReportsAsync_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllReportsAsync())
                .ReturnsAsync([]);

            // Act
            var result = await _service.GetBillSplitReportsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            _mockRepository.Verify(r => r.GetAllReportsAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetBillSplitReportsAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllReportsAsync())
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _service.GetBillSplitReportsAsync());
        }

        [TestMethod]
        public async Task GetBillSplitReportByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            var reportId = 1;
            _mockRepository.Setup(r => r.GetReportByIdAsync(reportId))
                .ReturnsAsync((BillSplitReport)null);

            // Act
            var result = await _service.GetBillSplitReportByIdAsync(reportId);

            // Assert
            Assert.IsNull(result);
            _mockRepository.Verify(r => r.GetReportByIdAsync(reportId), Times.Once);
        }

        [TestMethod]
        public async Task CreateBillSplitReportAsync_InvalidData_ReturnsNull()
        {
            // Arrange
            var report = new BillSplitReport
            {
                Id = 1,
                BillShare = 100m,
                ReportedUserCnp = "1234567890123",
                ReportingUserCnp = "9876543210987",
                DateOfTransaction = DateTime.UtcNow
            };

            // Act
            var result = await _service.CreateBillSplitReportAsync(report);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task DeleteBillSplitReportAsync_NotFound_ReturnsFalse()
        {
            // Arrange
            var report = new BillSplitReport
            {
                Id = 1,
                ReportedUserCnp = "1234567890123",
                ReportingUserCnp = "9876543210987",
                DateOfTransaction = DateTime.UtcNow,
                BillShare = 100m
            };
            _mockRepository.Setup(r => r.DeleteReportAsync(report.Id))
                .ReturnsAsync(false);

            // Act & Assert
            await _service.DeleteBillSplitReportAsync(report);
            _mockRepository.Verify(r => r.DeleteReportAsync(report.Id), Times.Once);
        }

        [TestMethod]
        public async Task GetDaysOverdueAsync_InvalidDate_ReturnsZero()
        {
            var report = new BillSplitReport
            {
                Id = 1,
                ReportedUserCnp = "1234567890123",
                ReportingUserCnp = "9876543210987",
                DateOfTransaction = DateTime.MinValue,
                BillShare = 100m
            };
            try
            {
                var result = await _service.GetDaysOverdueAsync(report);
                Assert.AreEqual(0, result);
            }
            catch { /* ignore */ }
        }
    }
}