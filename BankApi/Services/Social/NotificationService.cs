using BankApi.Repositories.Social;
using Common.Models.Social;
using Common.Services.Social;

namespace BankApi.Services.Social
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        }

        public async Task<List<Notification>> GetNotificationsForUser(int userId)
        {
            return await _notificationRepository.GetNotificationsByUserIdAsync(userId);
        }

        public async Task<Notification> GetNotificationById(int notificationId)
        {
            return await _notificationRepository.GetNotificationByIdAsync(notificationId);
        }

        public async Task CreateNotification(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            notification.Timestamp = DateTime.UtcNow;
            await _notificationRepository.CreateNotificationAsync(notification);
        }

        public async Task MarkNotificationAsRead(int notificationId, int userId)
        {
            var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId);
            var notification = notifications.FirstOrDefault(n => n.NotificationID == notificationId);

            if (notification != null)
            {
                await _notificationRepository.DeleteNotificationAsync(notificationId);
            }
        }

        public async Task MarkAllNotificationsAsRead(int userId)
        {
            var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId);
            foreach (var notification in notifications)
            {
                await _notificationRepository.DeleteNotificationAsync(notification.NotificationID);
            }
        }
    }
}