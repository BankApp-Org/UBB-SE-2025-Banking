using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Common.Models;

namespace Common.DTOs
{
    //[JsonPolymorphic(TypeDiscriminatorPropertyName = "messageType")]
    //[JsonDerivedType(typeof(TextMessageViewModel), "Text")]
    //[JsonDerivedType(typeof(ImageMessageViewModel), "Image")]
    //[JsonDerivedType(typeof(TransferMessageViewModel), "Transfer")]
    //[JsonDerivedType(typeof(RequestMessageViewModel), "Request")]
    public abstract class MessageViewModel
    {
        public int MessageID { get; set; }
        public int SenderID { get; set; }
        public int ChatID { get; set; }
        public string Timestamp { get; set; }
        // ???????
        public string SenderUsername { get; set; }
        public string MessageType { get; set; }
    }

    public class TextMessageViewModel : MessageViewModel
    {
        public string Content { get; set; }
        public List<User> UsersReport { get; set; }
    }

    public class ImageMessageViewModel : MessageViewModel
    {
        public string ImageURL { get; set; }
        public List<User> UsersReport { get; set; }
    }

    public class TransferMessageViewModel : MessageViewModel
    {
        public string Status { get; set; }
        public float Amount { get; set; }
        public string Description { get; set; }
        public string Currency { get; set; }
        public List<User> ListOfReceivers { get; set; }
    }

    public class RequestMessageViewModel : MessageViewModel
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