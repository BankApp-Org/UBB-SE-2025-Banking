using BankApi.Data;
using Common.Models.Social;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Repositories.Impl.Social
{
    public class NotificationRepository
    {
        private readonly ApiDbContext _context;
        public NotificationRepository(ApiDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Notification>> GetNotificationsByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();
        }

        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification), "Notification cannot be null.");
            }
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<bool> UpdateNotificationAsync(Notification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification), "Notification cannot be null.");
            }
            _context.Notifications.Update(notification);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
