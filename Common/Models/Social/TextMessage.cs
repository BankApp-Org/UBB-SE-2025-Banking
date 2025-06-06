using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Common.Models.Social
{
    public class TextMessage : Message
    {
        public TextMessage()
        {
            Type = MessageType.Text;
            UsersReport = [];
        }

        public TextMessage(int userId, int chatId, string messageContent, DateTime createdAt, List<User> usersReport)
        {
            UserId = userId;
            ChatId = chatId;
            MessageContent = messageContent;
            CreatedAt = createdAt;
            Type = MessageType.Text;
            UsersReport = usersReport ?? [];
        }

        [JsonConstructor]
        public TextMessage(int userId, int chatId, string messageContent, string createdAt, MessageType type, List<User> usersReport)
        {
            UserId = userId;
            ChatId = chatId;
            MessageContent = messageContent;
            CreatedAt = DateTime.Parse(createdAt);
            Type = type;
            UsersReport = usersReport ?? [];
        }

        public List<User> UsersReport { get; set; } = [];

        public string GetContent() => MessageContent;

        public override string ToString() => MessageContent;
    }
}
