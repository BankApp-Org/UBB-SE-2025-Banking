using Common.Models.Social;

namespace BankApi.Repositories.Social
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetNotificationsByUserIdAsync(int userId);
        Task<Notification> GetNotificationByIdAsync(int notificationId);
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<bool> UpdateNotificationAsync(Notification notification);
        Task<bool> DeleteNotificationAsync(int notificationId);
    }
} 