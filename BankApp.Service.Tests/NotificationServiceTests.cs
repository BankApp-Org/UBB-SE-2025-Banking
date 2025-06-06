using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using BankApi.Services.Social;
using BankApi.Repositories.Social;
using Common.Models.Social;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Common.Models;
using System.Runtime.Versioning;

namespace BankApp.Service.Tests
{
    [TestClass]
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class NotificationServiceTests
    {
        private Mock<INotificationRepository> _notificationRepoMock;
        private NotificationService _service;

        [TestInitialize]
        public void Setup()
        {
            _notificationRepoMock = new Mock<INotificationRepository>();
            _service = new NotificationService(_notificationRepoMock.Object);
        }

        private User CreateValidUser(int id = 1)
        {
            return new User { Id = id, CNP = $"CNP{id}", FirstName = "Test", LastName = "User" };
        }
        private Notification CreateValidNotification(int id = 1)
        {
            return new Notification { NotificationID = id, UserId = 1, Content = "Test", Timestamp = DateTime.UtcNow, User = CreateValidUser() };
        }

        [TestMethod]
        public async Task GetNotificationsForUser_ReturnsNotifications()
        {
            var notifications = new List<Notification> { CreateValidNotification(1) };
            _notificationRepoMock.Setup(r => r.GetNotificationsByUserIdAsync(1)).ReturnsAsync(notifications);
            var result = await _service.GetNotificationsForUser(1);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetNotificationById_ReturnsNotification()
        {
            var notification = CreateValidNotification(1);
            _notificationRepoMock.Setup(r => r.GetNotificationByIdAsync(1)).ReturnsAsync(notification);
            var result = await _service.GetNotificationById(1);
            Assert.AreEqual(1, result.NotificationID);
        }

        [TestMethod]
        public async Task CreateNotification_ValidNotification_CreatesSuccessfully()
        {
            var notification = CreateValidNotification(1);
            _notificationRepoMock.Setup(r => r.CreateNotificationAsync(notification)).ReturnsAsync(notification);
            await _service.CreateNotification(notification);
            _notificationRepoMock.Verify(r => r.CreateNotificationAsync(It.Is<Notification>(n => n.NotificationID == 1)), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateNotification_NullNotification_Throws()
        {
            await _service.CreateNotification(null);
        }

        [TestMethod]
        public async Task MarkNotificationAsRead_NotificationExists_DeletesNotification()
        {
            var notifications = new List<Notification> { CreateValidNotification(1) };
            _notificationRepoMock.Setup(r => r.GetNotificationsByUserIdAsync(1)).ReturnsAsync(notifications);
            _notificationRepoMock.Setup(r => r.DeleteNotificationAsync(1)).ReturnsAsync(true);
            await _service.MarkNotificationAsRead(1, 1);
            _notificationRepoMock.Verify(r => r.DeleteNotificationAsync(1), Times.Once);
        }

        [TestMethod]
        public async Task MarkNotificationAsRead_NotificationNotExists_DoesNothing()
        {
            var notifications = new List<Notification>();
            _notificationRepoMock.Setup(r => r.GetNotificationsByUserIdAsync(1)).ReturnsAsync(notifications);
            await _service.MarkNotificationAsRead(1, 1);
            _notificationRepoMock.Verify(r => r.DeleteNotificationAsync(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task MarkAllNotificationsAsRead_DeletesAllNotifications()
        {
            var notifications = new List<Notification> {
                CreateValidNotification(1),
                CreateValidNotification(2)
            };
            _notificationRepoMock.Setup(r => r.GetNotificationsByUserIdAsync(1)).ReturnsAsync(notifications);
            _notificationRepoMock.Setup(r => r.DeleteNotificationAsync(It.IsAny<int>())).ReturnsAsync(true);
            await _service.MarkAllNotificationsAsRead(1);
            _notificationRepoMock.Verify(r => r.DeleteNotificationAsync(1), Times.Once);
            _notificationRepoMock.Verify(r => r.DeleteNotificationAsync(2), Times.Once);
        }
    }
}