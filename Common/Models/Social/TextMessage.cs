using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Common.Models.Social
{
    public class TextMessage : Message
    {
        public TextMessage()
        {
            Type = MessageType.Text;
            UsersReport = new List<User>();
        }

        public TextMessage(int userId, int chatId, string messageContent, DateTime createdAt, List<User> usersReport)
        {
            UserId = userId;
            ChatId = chatId;
            MessageContent = messageContent;
            CreatedAt = createdAt;
            Type = MessageType.Text;
            UsersReport = usersReport ?? new();
        }

        [JsonConstructor]
        public TextMessage( int userId, int chatId, string messageContent, string createdAt, MessageType type, List<User> usersReport)
        {
            UserId = userId;
            ChatId = chatId;
            MessageContent = messageContent;
            CreatedAt = DateTime.Parse(createdAt);
            Type = type;
            UsersReport = usersReport ?? new();
        }

        public List<User> UsersReport { get; set; } = new();

        public string GetContent() => MessageContent;

        public override string ToString() => MessageContent;
    }
}
