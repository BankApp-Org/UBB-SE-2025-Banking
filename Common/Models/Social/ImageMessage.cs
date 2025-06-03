using System.Text.Json.Serialization;

namespace Common.Models.Social
{
    public class ImageMessage : Message
    {
        public ImageMessage()
        {
            Type = MessageType.Image;
        }
        public ImageMessage(int userId, int chatId, string imageUrl, List<User> usersReport, DateTime createdAt)
        {
            UserId = userId;
            ChatId = chatId;
            ImageUrl = imageUrl;
            UsersReport = usersReport ?? new List<User>();
            CreatedAt = createdAt;
            Type = MessageType.Image;
        }
        [JsonConstructor]
        public ImageMessage( int userId, int chatId, string imageUrl, List<User> usersReport, string createdAt)
        {
            UserId = userId;
            ChatId = chatId;
            ImageUrl = imageUrl;
            UsersReport = usersReport ?? new List<User>();
            CreatedAt = DateTime.Parse(createdAt);
            Type = MessageType.Image;
        }
        public string ImageUrl { get; set; } = string.Empty;
        public List<User> UsersReport { get; set; } = new();
        public override string ToString() => ImageUrl;
    }
}