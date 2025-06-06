using BankApi.Data;
using BankApi.Repositories.Impl.Social;
using Common.Models;
using Common.Models.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankApp.Repository.Tests
{
    [TestClass]
    public class NotificationRepositoryTests
    {
        private ApiDbContext _context;
        private NotificationRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApiDbContext(options);
            _repository = new NotificationRepository(_context);
        }

        private User CreateValidUser(int id = 1)
        {
            return new User { Id = id, CNP = $"CNP{id}", FirstName = "Test", LastName = "User" };
        }
        private Notification CreateValidNotification(int id = 1)
        {
            return new Notification
            {
                NotificationID = id,
                UserId = 1,
                Content = "Test Notification",
                Timestamp = DateTime.UtcNow,
                User = CreateValidUser()
            };
        }

        [TestMethod]
        public async Task CreateNotificationAsync_AddsNotification()
        {
            var notification = CreateValidNotification();
            var result = await _repository.CreateNotificationAsync(notification);
            Assert.IsNotNull(result);
            Assert.AreEqual(notification.Content, result.Content);
        }

        [TestMethod]
        public async Task GetNotificationByIdAsync_ReturnsNotification()
        {
            var notification = CreateValidNotification();
            _context.Notifications.Add(notification);
            _context.SaveChanges();
            var result = await _repository.GetNotificationByIdAsync(notification.NotificationID);
            Assert.IsNotNull(result);
            Assert.AreEqual(notification.NotificationID, result.NotificationID);
        }

        [TestMethod]
        public async Task GetNotificationsByUserIdAsync_ReturnsNotifications()
        {
            var notification = CreateValidNotification();
            _context.Notifications.Add(notification);
            _context.SaveChanges();
            var result = await _repository.GetNotificationsByUserIdAsync(notification.UserId);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task UpdateNotificationAsync_UpdatesNotification()
        {
            var notification = CreateValidNotification();
            _context.Notifications.Add(notification);
            _context.SaveChanges();
            notification.Content = "Updated Content";
            var updated = await _repository.UpdateNotificationAsync(notification);
            Assert.IsTrue(updated);
            var fetched = await _repository.GetNotificationByIdAsync(notification.NotificationID);
            Assert.AreEqual("Updated Content", fetched.Content);
        }

        [TestMethod]
        public async Task DeleteNotificationAsync_DeletesNotification()
        {
            var notification = CreateValidNotification();
            _context.Notifications.Add(notification);
            _context.SaveChanges();
            var deleted = await _repository.DeleteNotificationAsync(notification.NotificationID);
            Assert.IsTrue(deleted);
            Assert.AreEqual(0, _context.Notifications.Count());
        }
    }
} 