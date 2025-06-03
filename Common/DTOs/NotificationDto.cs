using Common.Models;

namespace Common.DTOs
{
    public class NotificationDto
    {
        public int NotificationID { get; set; }

        public DateTime Timestamp { get; set; }

        public string Content { get; set; }

        public User UserReceiver { get; set; }
    }
}