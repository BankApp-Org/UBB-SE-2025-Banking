namespace Common.DTOs
{
    public class NotificationViewModel
    {
        public int NotificationID { get; set; }

        public DateTime Timestamp { get; set; }

        public string Content { get; set; }

        public int UserReceiverID { get; set; }
    }

    public class FriendNotificationDto
    {
        public int UserId { get; set; }

        public int NewFriendId { get; set; }

        public int OldFriendId { get; set; }
    }

    public class MessageNotificationDto
    {
        public int MessageSenderId { get; set; }

        public int ChatId { get; set; }
    }

    public class TransactionNotificationDto
    {
        public int ReceiverId { get; set; }

        public int ChatId { get; set; }

        public string Type { get; set; }

        public float Amount { get; set; }

        public string Currency { get; set; }
    }

    public class NewChatNotificationDto
    {
        public int ChatId { get; set; }
    }
}