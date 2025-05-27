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
    public class TipsServiceTests
    {
        private Mock<ITipsRepository> _tipsRepositoryMock;
        private Mock<IUserRepository> _userRepositoryMock;
        private TipsService _sut;

        [TestInitialize]
        public void TestInitialize()
        {
            _tipsRepositoryMock = new Mock<ITipsRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _sut = new TipsService(_tipsRepositoryMock.Object, _userRepositoryMock.Object);
        }

        [TestMethod]
        public async Task GiveTipToUserAsync_LowCreditScore_CallsGiveLowBracketTipAsync()
        {
            // Arrange
            string cnp = "123";
            var user = new User { CNP = cnp, CreditScore = 250 };
            _userRepositoryMock.Setup(r => r.GetByCnpAsync(cnp)).ReturnsAsync(user);

            // Act
            await _sut.GiveTipToUserAsync(cnp);

            // Assert
            _tipsRepositoryMock.Verify(r => r.GiveLowBracketTipAsync(cnp), Times.Once);
            _tipsRepositoryMock.Verify(r => r.GiveMediumBracketTipAsync(It.IsAny<string>()), Times.Never);
            _tipsRepositoryMock.Verify(r => r.GiveHighBracketTipAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task GiveTipToUserAsync_MediumCreditScore_CallsGiveMediumBracketTipAsync()
        {
            // Arrange
            string cnp = "456";
            var user = new User { CNP = cnp, CreditScore = 400 };
            _userRepositoryMock.Setup(r => r.GetByCnpAsync(cnp)).ReturnsAsync(user);

            // Act
            await _sut.GiveTipToUserAsync(cnp);

            // Assert
            _tipsRepositoryMock.Verify(r => r.GiveMediumBracketTipAsync(cnp), Times.Once);
            _tipsRepositoryMock.Verify(r => r.GiveLowBracketTipAsync(It.IsAny<string>()), Times.Never);
            _tipsRepositoryMock.Verify(r => r.GiveHighBracketTipAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task GiveTipToUserAsync_HighCreditScore_CallsGiveHighBracketTipAsync()
        {
            // Arrange
            string cnp = "789";
            var user = new User { CNP = cnp, CreditScore = 700 };
            _userRepositoryMock.Setup(r => r.GetByCnpAsync(cnp)).ReturnsAsync(user);

            // Act
            await _sut.GiveTipToUserAsync(cnp);

            // Assert
            _tipsRepositoryMock.Verify(r => r.GiveHighBracketTipAsync(cnp), Times.Once);
            _tipsRepositoryMock.Verify(r => r.GiveLowBracketTipAsync(It.IsAny<string>()), Times.Never);
            _tipsRepositoryMock.Verify(r => r.GiveMediumBracketTipAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task GiveTipToUserAsync_UserNotFound_DoesNotCallAnyTipMethod()
        {
            // Arrange
            string cnp = "000";
            _userRepositoryMock.Setup(r => r.GetByCnpAsync(cnp)).ReturnsAsync((User)null);

            // Act
            await _sut.GiveTipToUserAsync(cnp);

            // Assert
            _tipsRepositoryMock.Verify(r => r.GiveLowBracketTipAsync(It.IsAny<string>()), Times.Never);
            _tipsRepositoryMock.Verify(r => r.GiveMediumBracketTipAsync(It.IsAny<string>()), Times.Never);
            _tipsRepositoryMock.Verify(r => r.GiveHighBracketTipAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task GiveTipToUserAsync_RepositoryThrowsException_DoesNotThrow()
        {
            // Arrange
            string cnp = "errorCnp";
            _userRepositoryMock.Setup(r => r.GetByCnpAsync(cnp)).ThrowsAsync(new Exception("Repo failure"));

            // Act + Assert: Should not throw due to internal try/catch
            await _sut.GiveTipToUserAsync(cnp);

            // Also verify no tip methods are called
            _tipsRepositoryMock.Verify(r => r.GiveLowBracketTipAsync(It.IsAny<string>()), Times.Never);
            _tipsRepositoryMock.Verify(r => r.GiveMediumBracketTipAsync(It.IsAny<string>()), Times.Never);
            _tipsRepositoryMock.Verify(r => r.GiveHighBracketTipAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task GetTipsForUserAsync_ValidCnp_ReturnsExpectedTips()
        {
            // Arrange
            string cnp = "user123";
            var expected = new List<Tip> { new() { Id = 1 }, new() { Id = 2 } };
            _tipsRepositoryMock.Setup(r => r.GetTipsForUserAsync(cnp)).ReturnsAsync(expected);

            // Act
            var result = await _sut.GetTipsForUserAsync(cnp);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].Id);
        }

        [TestMethod]
        public async Task GetTipsForUserAsync_RepositoryThrows_ExceptionBubblesUp()
        {
            // Arrange
            string cnp = "errorCnp";
            _tipsRepositoryMock.Setup(r => r.GetTipsForUserAsync(cnp)).ThrowsAsync(new Exception("Something went wrong"));

            // Act
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _sut.GetTipsForUserAsync(cnp)); // Expect exception
        }
    }
}
