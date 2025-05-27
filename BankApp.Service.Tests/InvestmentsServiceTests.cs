using BankApi.Repositories;
using BankApi.Services;
using Common.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace StockApp.Service.Tests
{
    [TestClass]
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class InvestmentsServiceTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IInvestmentsRepository> _mockInvestmentsRepository;
        private InvestmentsService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockInvestmentsRepository = new Mock<IInvestmentsRepository>();
            _service = new InvestmentsService(_mockUserRepository.Object, _mockInvestmentsRepository.Object);
        }

        [TestMethod]
        public async Task GetInvestmentsHistoryAsync_ReturnsInvestments_WhenRepositorySucceeds()
        {
            // Arrange
            var expectedInvestments = new List<Investment>
            {
                new Investment { Id = 1, InvestorCnp = "123", AmountInvested = 1000 },
                new Investment { Id = 2, InvestorCnp = "456", AmountInvested = 2000 }
            };
            _mockInvestmentsRepository.Setup(x => x.GetInvestmentsHistory()).ReturnsAsync(expectedInvestments);

            // Act
            var result = await _service.GetInvestmentsHistoryAsync();

            // Assert
            CollectionAssert.AreEqual(expectedInvestments, result);
            _mockInvestmentsRepository.Verify(x => x.GetInvestmentsHistory(), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetInvestmentsHistoryAsync_ThrowsException_WhenRepositoryFails()
        {
            // Arrange
            _mockInvestmentsRepository.Setup(x => x.GetInvestmentsHistory())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            await _service.GetInvestmentsHistoryAsync();
        }

        [TestMethod]
        public async Task AddInvestmentAsync_Success_WhenInputIsValid()
        {
            // Arrange
            var investment = new Investment { InvestorCnp = "123", AmountInvested = 1000 };
            _mockInvestmentsRepository.Setup(x => x.AddInvestment(investment)).Returns(Task.CompletedTask);

            // Act
            await _service.AddInvestmentAsync(investment);

            // Assert
            _mockInvestmentsRepository.Verify(x => x.AddInvestment(investment), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddInvestmentAsync_ThrowsArgumentNull_WhenInvestmentIsNull()
        {
            // Act
            await _service.AddInvestmentAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task AddInvestmentAsync_ThrowsException_WhenRepositoryFails()
        {
            // Arrange
            var investment = new Investment { InvestorCnp = "123", AmountInvested = 1000 };
            _mockInvestmentsRepository.Setup(x => x.AddInvestment(investment))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            await _service.AddInvestmentAsync(investment);
        }

        [TestMethod]
        public async Task UpdateInvestmentAsync_Success_WhenInputIsValid()
        {
            // Arrange
            _mockInvestmentsRepository.Setup(x => x.UpdateInvestment(1, "123", 500m))
                .Returns(Task.CompletedTask);

            // Act
            await _service.UpdateInvestmentAsync(1, "123", 500m);

            // Assert
            _mockInvestmentsRepository.Verify(x => x.UpdateInvestment(1, "123", 500m), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdateInvestmentAsync_ThrowsArgumentException_WhenCNPIsEmpty()
        {
            // Act
            await _service.UpdateInvestmentAsync(1, "", 500m);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task UpdateInvestmentAsync_ThrowsException_WhenRepositoryFails()
        {
            // Arrange
            _mockInvestmentsRepository.Setup(x => x.UpdateInvestment(1, "123", 500m))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            await _service.UpdateInvestmentAsync(1, "123", 500m);
        }

        [TestMethod]
        public async Task CalculateAndUpdateRiskScoreAsync_UpdatesAllUsers_WhenSuccessful()
        {
            // Arrange
            var users = new List<User>
            {
                new User { CNP = "123", RiskScore = 50, Income = 5000 },
                new User { CNP = "456", RiskScore = 30, Income = 3000 }
            };

            var investments = new List<Investment>
            {
                new Investment { InvestorCnp = "123", AmountInvested = 1000, AmountReturned = 1200, InvestmentDate = DateTime.Now.AddDays(-1) },
                new Investment { InvestorCnp = "123", AmountInvested = 500, AmountReturned = 400, InvestmentDate = DateTime.Now }
            };

            _mockUserRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(users);
            _mockInvestmentsRepository.Setup(x => x.GetInvestmentsHistory()).ReturnsAsync(investments);

            // Act
            await _service.CalculateAndUpdateRiskScoreAsync();

            // Assert
            _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Exactly(users.Count));
            Assert.IsTrue(users[0].RiskScore != 50); // Risk score should have changed
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task CalculateAndUpdateRiskScoreAsync_ThrowsException_WhenRepositoryFails()
        {
            // Arrange
            _mockUserRepository.Setup(x => x.GetAllAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            await _service.CalculateAndUpdateRiskScoreAsync();
        }

        [TestMethod]
        public async Task CalculateAndUpdateROIAsync_UpdatesAllUsers_WhenSuccessful()
        {
            // Arrange
            var users = new List<User>
            {
                new User { CNP = "123", ROI = 1 },
                new User { CNP = "456", ROI = 1 }
            };

            var investments = new List<Investment>
            {
                new Investment { InvestorCnp = "123", AmountInvested = 1000, AmountReturned = 1200 },
                new Investment { InvestorCnp = "456", AmountInvested = 500, AmountReturned = 400 }
            };

            _mockUserRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(users);
            _mockInvestmentsRepository.Setup(x => x.GetInvestmentsHistory()).ReturnsAsync(investments);

            // Act
            await _service.CalculateAndUpdateROIAsync();

            // Assert
            _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Exactly(users.Count));
            Assert.AreEqual(1.2m, users[0].ROI); // 1200/1000 = 1.2
            Assert.AreEqual(0.8m, users[1].ROI); // 400/500 = 0.8
        }

        [TestMethod]
        public async Task CreditScoreUpdateInvestmentsBasedAsync_UpdatesScoresCorrectly()
        {
            // Arrange
            var users = new List<User>
            {
                new User { CNP = "123", CreditScore = 500, RiskScore = 50, ROI = 1.2m },
                new User { CNP = "456", CreditScore = 500, RiskScore = 80, ROI = 0.8m }
            };

            _mockUserRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(users);

            // Act
            await _service.CreditScoreUpdateInvestmentsBasedAsync();

            // Assert
            _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Exactly(users.Count));

            // First user should have increased credit score (ROI > 1)
            Assert.IsTrue(users[0].CreditScore > 500);

            // Second user should have decreased credit score (ROI < 1)
            Assert.IsTrue(users[1].CreditScore < 500);
        }

        [TestMethod]
        public async Task GetPortfolioSummaryAsync_ReturnsCorrectSummary()
        {
            // Arrange
            var users = new List<User>
            {
                new User { CNP = "123", FirstName = "John", LastName = "Doe", RiskScore = 50 },
                new User { CNP = "456", FirstName = "Jane", LastName = "Smith", RiskScore = 30 }
            };

            var investments = new List<Investment>
            {
                new Investment { InvestorCnp = "123", AmountInvested = 1000, AmountReturned = 1200 },
                new Investment { InvestorCnp = "123", AmountInvested = 500, AmountReturned = 400 },
                new Investment { InvestorCnp = "456", AmountInvested = 2000, AmountReturned = 2500 }
            };

            _mockUserRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(users);
            _mockInvestmentsRepository.Setup(x => x.GetInvestmentsHistory()).ReturnsAsync(investments);

            // Act
            var result = await _service.GetPortfolioSummaryAsync();

            // Assert
            Assert.AreEqual(2, result.Count);

            var johnPortfolio = result.First(p => p.FirstName == "John");
            Assert.AreEqual(1500m, johnPortfolio.TotalAmountInvested);
            Assert.AreEqual(1600m, johnPortfolio.TotalAmountReturned);
            Assert.AreEqual(50, johnPortfolio.RiskFactor);
        }
    }
}