namespace Common.Models.Social
{
    public class TextMessage : Message
    {
        public TextMessage()
        {
            Type = MessageType.Text.ToString();
            MessageType = MessageType.Text;
            UsersReport = [];
        }

        public TextMessage(int userId, int chatId, string messageContent, DateTime createdAt, List<User> usersReport)
        {
            UserId = userId;
            ChatId = chatId;
            MessageContent = messageContent;
            CreatedAt = createdAt;
            Type = MessageType.Text.ToString();
            MessageType = MessageType.Text;
            UsersReport = usersReport ?? [];
        }

        public List<User> UsersReport { get; set; } = [];

        public string GetContent() => MessageContent;

        public override string ToString() => MessageContent;
    }
}
