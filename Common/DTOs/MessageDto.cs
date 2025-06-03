using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Common.Models;

namespace Common.DTOs
{
    //[JsonPolymorphic(TypeDiscriminatorPropertyName = "messageType")]
    //[JsonDerivedType(typeof(TextMessageDto), "Text")]
    //[JsonDerivedType(typeof(ImageMessageDto), "Image")]
    //[JsonDerivedType(typeof(TransferMessageDto), "Transfer")]
    //[JsonDerivedType(typeof(RequestMessageDto), "Request")]
    public abstract class MessageDto
    {
        public int MessageID { get; set; }
        public int SenderID { get; set; }
        public int ChatID { get; set; }
        public string Timestamp { get; set; }
        // ???????
        public string SenderUsername { get; set; }
        public string MessageType { get; set; }
    }

    public class TextMessageDto : MessageDto
    {
        public string Content { get; set; }
        public List<User> UsersReport { get; set; }
    }

    public class ImageMessageDto : MessageDto
    {
        public string ImageURL { get; set; }
        public List<User> UsersReport { get; set; }
    }

    public class TransferMessageDto : MessageDto
    {
        public string Status { get; set; }
        public float Amount { get; set; }
        public string Description { get; set; }
        public string Currency { get; set; }
        public List<User> ListOfReceivers { get; set; }
    }

    public class RequestMessageDto : MessageDto
    {
        public string Status { get; set; }
        public float Amount { get; set; }
        public string Description { get; set; }
        public string Currency { get; set; }
    }

    //public class MessageViewModel
    //{
    //    public int MessageID { get; set; }
    //    public int SenderID { get; set; }
    //    public int ChatID { get; set; }
    //    public string Timestamp { get; set; }
    //    // ???????
    //    public string SenderUsername { get; set; }
    //    public string MessageType { get; set; }
    //    public string? Content { get; set; }
    //    public List<int>? UsersReport { get; set; }
    //    public string? Status { get; set; }
    //    public float? Amount { get; set; }
    //    public string? Currency { get; set; }
    //    public List<int>? ListOfReceiversID { get; set; }
    //}
}