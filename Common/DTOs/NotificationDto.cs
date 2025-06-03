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

    public class FriendNotificationDto : NotificationDto
    {
        public User User { get; set; }

        public User NewFriend { get; set; }

        public User OldFriend { get; set; }
    }

    public class MessageNotificationDto : NotificationDto
    {
        public User MessageSender{ get; set; }

        public int ChatId { get; set; }
    }

    public class TransactionNotificationDto : NotificationDto 
    {
        public User Receiver { get; set; }

        public int ChatId { get; set; }

        public string Type { get; set; }

        public float Amount { get; set; }

        public string Currency { get; set; }
    }

    public class NewChatNotificationDto : NotificationDto
    {
        public int ChatId { get; set; }
    }
}