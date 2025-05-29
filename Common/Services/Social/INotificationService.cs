using Common.Models.Social;

namespace Common.Services.Social
{
    public interface INotificationService
    {
        Task<List<Notification>> GetNotificationsForUser(int userId);

        Task<Notification> GetNotificationById(int notificationId);

        Task MarkNotificationAsRead(int notificationId, int userId);

        Task MarkAllNotificationsAsRead(int userId);

        Task CreateNotification(Notification notification);
    }
}
