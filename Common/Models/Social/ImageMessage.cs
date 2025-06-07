namespace Common.Models.Social
{
    public class ImageMessage : Message
    {
        public ImageMessage()
        {
            Type = MessageType.Image.ToString();
            MessageType = MessageType.Image;
        }
        public ImageMessage(int userId, int chatId, string imageUrl, List<User> usersReport, DateTime createdAt)
        {
            UserId = userId;
            ChatId = chatId;
            ImageUrl = imageUrl;
            UsersReport = usersReport ?? [];
            CreatedAt = createdAt;
            Type = MessageType.Image.ToString();
            MessageType = MessageType.Image;
        }

        public string ImageUrl { get; set; } = string.Empty;
        public List<User> UsersReport { get; set; } = [];
        public override string ToString() => ImageUrl;
    }
}