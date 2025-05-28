using BankApi.Repositories;
using BankApi.Services;
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
    [SupportedOSPlatform("windows10.0.26100.0")]
    [TestClass]
    public class AlertServiceTests
    {
        private Mock<IAlertRepository> _mockRepository;
        private AlertService _service;

        [TestInitialize]
        public void Init()
        {
            _mockRepository = new Mock<IAlertRepository>();
            _service = new AlertService(_mockRepository.Object);
        }

        [TestMethod]
        public async Task GetAllAlertsAsync_Success()
        {
            // Arrange
            var expectedAlerts = new List<Alert>
            {
                new() { AlertId = 1, StockName = "AAPL", Name = "Test Alert 1" },
                new() { AlertId = 2, StockName = "MSFT", Name = "Test Alert 2" }
            };
            _mockRepository.Setup(r => r.GetAllAlertsAsync())
                .ReturnsAsync(expectedAlerts);

            // Act
            var result = await _service.GetAllAlertsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            _mockRepository.Verify(r => r.GetAllAlertsAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetAllAlertsOnAsync_Success()
        {
            // Arrange
            var alerts = new List<Alert>
            {
                new() { AlertId = 1, StockName = "AAPL", Name = "Test Alert 1", ToggleOnOff = true },
                new() { AlertId = 2, StockName = "MSFT", Name = "Test Alert 2", ToggleOnOff = false }
            };
            _mockRepository.Setup(r => r.GetAllAlertsAsync())
                .ReturnsAsync(alerts);

            // Act
            var result = await _service.GetAllAlertsOnAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].ToggleOnOff);
        }

        [TestMethod]
        public async Task GetAlertByIdAsync_Success()
        {
            // Arrange
            var alertId = 1;
            var expectedAlert = new Alert { AlertId = alertId, StockName = "AAPL", Name = "Test Alert" };
            _mockRepository.Setup(r => r.GetAlertByIdAsync(alertId))
                .ReturnsAsync(expectedAlert);

            // Act
            var result = await _service.GetAlertByIdAsync(alertId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(alertId, result.AlertId);
            _mockRepository.Verify(r => r.GetAlertByIdAsync(alertId), Times.Once);
        }

        [TestMethod]
        public async Task CreateAlertAsync_Success()
        {
            // Arrange
            var stockName = "AAPL";
            var name = "Test Alert";
            var upperBound = 200m;
            var lowerBound = 100m;
            var toggleOnOff = true;

            var expectedAlert = new Alert
            {
                StockName = stockName,
                Name = name,
                UpperBound = upperBound,
                LowerBound = lowerBound,
                ToggleOnOff = toggleOnOff
            };

            _mockRepository.Setup(r => r.AddAlertAsync(It.IsAny<Alert>()))
                .ReturnsAsync(expectedAlert);

            // Act
            var result = await _service.CreateAlertAsync(stockName, name, upperBound, lowerBound, toggleOnOff);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(stockName, result.StockName);
            Assert.AreEqual(name, result.Name);
            Assert.AreEqual(upperBound, result.UpperBound);
            Assert.AreEqual(lowerBound, result.LowerBound);
            Assert.AreEqual(toggleOnOff, result.ToggleOnOff);
            _mockRepository.Verify(r => r.AddAlertAsync(It.IsAny<Alert>()), Times.Once);
        }

        [TestMethod]
        public async Task CreateAlertAsync_EmptyStockName_ReturnsNull()
        {
            // Act
            var result = await _service.CreateAlertAsync("", "Test Alert", 200m, 100m, true);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CreateAlertAsync_NullStockName_ReturnsNull()
        {
            // Act
            var result = await _service.CreateAlertAsync(null, "Test Alert", 200m, 100m, true);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CreateAlertAsync_EmptyName_ReturnsNull()
        {
            // Act
            var result = await _service.CreateAlertAsync("AAPL", "", 200m, 100m, true);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CreateAlertAsync_NullName_ReturnsNull()
        {
            // Act
            var result = await _service.CreateAlertAsync("AAPL", null, 200m, 100m, true);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CreateAlertAsync_InvalidBounds_ReturnsNull()
        {
            // Act
            var result = await _service.CreateAlertAsync("AAPL", "Test Alert", 100m, 200m, true);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task UpdateAlertAsync_Success()
        {
            // Arrange
            var alertId = 1;
            var stockName = "AAPL";
            var name = "Updated Alert";
            var upperBound = 300m;
            var lowerBound = 150m;
            var toggleOnOff = false;

            var existingAlert = new Alert { AlertId = alertId };
            _mockRepository.Setup(r => r.GetAlertByIdAsync(alertId))
                .ReturnsAsync(existingAlert);

            // Act
            await _service.UpdateAlertAsync(alertId, stockName, name, upperBound, lowerBound, toggleOnOff);

            // Assert
            _mockRepository.Verify(r => r.UpdateAlertAsync(It.Is<Alert>(a =>
                a.AlertId == alertId &&
                a.StockName == stockName &&
                a.Name == name &&
                a.UpperBound == upperBound &&
                a.LowerBound == lowerBound &&
                a.ToggleOnOff == toggleOnOff)), Times.Once);
        }

        [TestMethod]
        public async Task UpdateAlertAsync_EmptyStockName_ReturnsNull()
        {
            // Act & Assert
            await _service.UpdateAlertAsync(1, "", "Test Alert", 200m, 100m, true);
            _mockRepository.Verify(r => r.UpdateAlertAsync(It.IsAny<Alert>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateAlertAsync_NullStockName_ReturnsNull()
        {
            // Act & Assert
            await _service.UpdateAlertAsync(1, null, "Test Alert", 200m, 100m, true);
            _mockRepository.Verify(r => r.UpdateAlertAsync(It.IsAny<Alert>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateAlertAsync_EmptyName_ReturnsNull()
        {
            // Act & Assert
            await _service.UpdateAlertAsync(1, "AAPL", "", 200m, 100m, true);
            _mockRepository.Verify(r => r.UpdateAlertAsync(It.IsAny<Alert>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateAlertAsync_NullName_ReturnsNull()
        {
            // Act & Assert
            await _service.UpdateAlertAsync(1, "AAPL", null, 200m, 100m, true);
            _mockRepository.Verify(r => r.UpdateAlertAsync(It.IsAny<Alert>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateAlertAsync_InvalidBounds_ReturnsNull()
        {
            // Act & Assert
            await _service.UpdateAlertAsync(1, "AAPL", "Test Alert", 100m, 200m, true);
            _mockRepository.Verify(r => r.UpdateAlertAsync(It.IsAny<Alert>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateAlertAsync_InvalidId_ReturnsNull()
        {
            // Act & Assert
            await _service.UpdateAlertAsync(0, "AAPL", "Test Alert", 200m, 100m, true);
            _mockRepository.Verify(r => r.UpdateAlertAsync(It.IsAny<Alert>()), Times.Never);
        }

        [TestMethod]
        public async Task RemoveAlertAsync_Success()
        {
            // Arrange
            var alertId = 1;

            // Act
            await _service.RemoveAlertAsync(alertId);

            // Assert
            _mockRepository.Verify(r => r.DeleteAlertAsync(alertId), Times.Once);
        }

        [TestMethod]
        public async Task GetTriggeredAlertsAsync_Success()
        {
            // Arrange
            var expectedTriggeredAlerts = new List<TriggeredAlert>
            {
                new() { Id = 1, StockName = "AAPL", Message = "Test Alert 1" },
                new() { Id = 2, StockName = "MSFT", Message = "Test Alert 2" }
            };
            _mockRepository.Setup(r => r.GetTriggeredAlertsAsync())
                .ReturnsAsync(expectedTriggeredAlerts);

            // Act
            var result = await _service.GetTriggeredAlertsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("AAPL", result[0].StockName);
            Assert.AreEqual("MSFT", result[1].StockName);
            _mockRepository.Verify(r => r.GetTriggeredAlertsAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetAlertByIdAsync_InvalidId_ReturnsNull()
        {
            // Act
            var result = await _service.GetAlertByIdAsync(0);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetAllAlertsAsync_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAlertsAsync())
                .ReturnsAsync([]);

            // Act
            var result = await _service.GetAllAlertsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            _mockRepository.Verify(r => r.GetAllAlertsAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetAllAlertsAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAlertsAsync())
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _service.GetAllAlertsAsync());
        }

        [TestMethod]
        public async Task GetAllAlertsOnAsync_NoActiveAlerts_ReturnsEmptyList()
        {
            // Arrange
            var alerts = new List<Alert>
            {
                new() { AlertId = 1, StockName = "AAPL", Name = "Test Alert 1", ToggleOnOff = false },
                new() { AlertId = 2, StockName = "MSFT", Name = "Test Alert 2", ToggleOnOff = false }
            };
            _mockRepository.Setup(r => r.GetAllAlertsAsync())
                .ReturnsAsync(alerts);

            // Act
            var result = await _service.GetAllAlertsOnAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetAllAlertsOnAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAlertsAsync())
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _service.GetAllAlertsOnAsync());
        }

        [TestMethod]
        public async Task GetAlertByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            var alertId = 1;
            _mockRepository.Setup(r => r.GetAlertByIdAsync(alertId))
                .ReturnsAsync((Alert)null);

            // Act
            var result = await _service.GetAlertByIdAsync(alertId);

            // Assert
            Assert.IsNull(result);
            _mockRepository.Verify(r => r.GetAlertByIdAsync(alertId), Times.Once);
        }

        [TestMethod]
        public async Task RemoveAlertAsync_NotFound_ReturnsFalse()
        {
            // Arrange
            var alertId = 1;
            _mockRepository.Setup(r => r.DeleteAlertAsync(alertId))
                .ReturnsAsync(false);

            // Act & Assert
            await _service.RemoveAlertAsync(alertId);
            _mockRepository.Verify(r => r.DeleteAlertAsync(alertId), Times.Once);
        }

        [TestMethod]
        public async Task GetTriggeredAlertsAsync_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetTriggeredAlertsAsync())
                .ReturnsAsync([]);

            // Act
            var result = await _service.GetTriggeredAlertsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            _mockRepository.Verify(r => r.GetTriggeredAlertsAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetTriggeredAlertsAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetTriggeredAlertsAsync())
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _service.GetTriggeredAlertsAsync());
        }
    }
}
