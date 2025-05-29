using BankApi.Repositories.Bank;
using BankApi.Services.Stock;
using Common.Models.Bank;
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
    public class ActivityServiceTests
    {
        private Mock<IActivityRepository> _mockRepository;
        private ActivityService _service;

        [TestInitialize]
        public void Init()
        {
            _mockRepository = new Mock<IActivityRepository>();
            _service = new ActivityService(_mockRepository.Object);
        }

        [TestMethod]
        public async Task GetActivityForUser_Success()
        {
            // Arrange
            var userCnp = "1234567890123";
            var expectedActivities = new List<ActivityLog>
            {
                new() { Id = 1, UserCnp = userCnp, ActivityName = "Test Activity" }
            };
            _mockRepository.Setup(r => r.GetActivityForUserAsync(userCnp))
                .ReturnsAsync(expectedActivities);

            // Act
            var result = await _service.GetActivityForUser(userCnp);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(userCnp, result[0].UserCnp);
            _mockRepository.Verify(r => r.GetActivityForUserAsync(userCnp), Times.Once);
        }

        [TestMethod]
        public async Task GetActivityForUser_EmptyCnp_ThrowsException()
        {
            // Act
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _service.GetActivityForUser(""));
        }

        [TestMethod]
        public async Task AddActivity_Success()
        {
            // Arrange
            var userCnp = "1234567890123";
            var activityName = "Test Activity";
            var amount = 100;
            var details = "Test Details";
            var expectedActivity = new ActivityLog
            {
                UserCnp = userCnp,
                ActivityName = activityName,
                LastModifiedAmount = amount,
                ActivityDetails = details
            };

            _mockRepository.Setup(r => r.AddActivityAsync(It.IsAny<ActivityLog>()))
                .ReturnsAsync(expectedActivity);

            // Act
            var result = await _service.AddActivity(userCnp, activityName, amount, details);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(userCnp, result.UserCnp);
            Assert.AreEqual(activityName, result.ActivityName);
            Assert.AreEqual(amount, result.LastModifiedAmount);
            _mockRepository.Verify(r => r.AddActivityAsync(It.IsAny<ActivityLog>()), Times.Once);
        }

        [TestMethod]
        public async Task AddActivity_InvalidAmount_ThrowsException()
        {
            // Act
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _service.AddActivity("1234567890123", "Test", 0, "Details"));
        }

        [TestMethod]
        public async Task AddActivity_EmptyActivityName_ThrowsException()
        {
            // Act
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _service.AddActivity("1234567890123", "", 100, "Details"));
        }

        [TestMethod]
        public async Task GetAllActivities_Success()
        {
            // Arrange
            var expectedActivities = new List<ActivityLog>
            {
                new() { Id = 1, ActivityName = "Activity 1" },
                new() { Id = 2, ActivityName = "Activity 2" }
            };
            _mockRepository.Setup(r => r.GetAllActivitiesAsync())
                .ReturnsAsync(expectedActivities);

            // Act
            var result = await _service.GetAllActivities();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            _mockRepository.Verify(r => r.GetAllActivitiesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetActivityById_Success()
        {
            // Arrange
            var activityId = 1;
            var expectedActivity = new ActivityLog { Id = activityId, ActivityName = "Test Activity" };
            _mockRepository.Setup(r => r.GetActivityByIdAsync(activityId))
                .ReturnsAsync(expectedActivity);

            // Act
            var result = await _service.GetActivityById(activityId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(activityId, result.Id);
            _mockRepository.Verify(r => r.GetActivityByIdAsync(activityId), Times.Once);
        }

        [TestMethod]
        public async Task GetActivityById_InvalidId_ThrowsException()
        {
            // Act
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _service.GetActivityById(0));
        }

        [TestMethod]
        public async Task DeleteActivity_Success()
        {
            // Arrange
            var activityId = 1;
            _mockRepository.Setup(r => r.DeleteActivityAsync(activityId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteActivity(activityId);

            // Assert
            Assert.IsTrue(result);
            _mockRepository.Verify(r => r.DeleteActivityAsync(activityId), Times.Once);
        }

        [TestMethod]
        public async Task DeleteActivity_InvalidId_ThrowsException()
        {
            // Act
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _service.DeleteActivity(-1));
        }

        [TestMethod]
        public async Task GetActivityForUser_NullCnp_ThrowsException()
        {
            // Act
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _service.GetActivityForUser(null));
        }

        [TestMethod]
        public async Task AddActivity_NullActivityName_ThrowsException()
        {
            // Act
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _service.AddActivity("1234567890123", null, 100, "Details"));
        }

        [TestMethod]
        public async Task GetAllActivities_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllActivitiesAsync())
                .ReturnsAsync([]);

            // Act
            var result = await _service.GetAllActivities();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            _mockRepository.Verify(r => r.GetAllActivitiesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetAllActivities_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllActivitiesAsync())
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _service.GetAllActivities());
        }

        [TestMethod]
        public async Task GetActivityById_NotFound_ReturnsNull()
        {
            // Arrange
            var activityId = 1;
            _mockRepository.Setup(r => r.GetActivityByIdAsync(activityId))
                .ReturnsAsync((ActivityLog)null);

            // Act
            var result = await _service.GetActivityById(activityId);

            // Assert
            Assert.IsNull(result);
            _mockRepository.Verify(r => r.GetActivityByIdAsync(activityId), Times.Once);
        }

        [TestMethod]
        public async Task DeleteActivity_NotFound_ReturnsFalse()
        {
            // Arrange
            var activityId = 1;
            _mockRepository.Setup(r => r.DeleteActivityAsync(activityId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteActivity(activityId);

            // Assert
            Assert.IsFalse(result);
            _mockRepository.Verify(r => r.DeleteActivityAsync(activityId), Times.Once);
        }
    }
}