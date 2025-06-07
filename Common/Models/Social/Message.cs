using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Common.Models.Social
{
    [JsonDerivedType(typeof(TextMessage), "Text")]
    [JsonDerivedType(typeof(ImageMessage), "Image")]
    [JsonDerivedType(typeof(TransferMessage), "Transfer")]
    [JsonDerivedType(typeof(RequestMessage), "Request")]
    [JsonDerivedType(typeof(BillSplitMessage), "BillSplit")]
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
    public class Message
    {
        public string Type { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string MessageContent { get; set; } = string.Empty;

        public Message(int id, MessageType type, string message)
        {
            Id = id;
            MessageType = type;
            Type = type.ToString();
            MessageContent = message;
        }

        public Message()
        {
            Id = 0;
            MessageType = MessageType.Text;
            Type = MessageType.Text.ToString();
            MessageContent = string.Empty;
        }

        [Required]
        public int UserId { get; set; }

        [Required]
        public MessageType MessageType { get; set; }


        [Required]
        [ForeignKey("UserId")]
        public User Sender { get; set; } = null!;

        [Required]
        public int ChatId { get; set; }

        [Required]
        [ForeignKey("ChatId")]
        public Chat Chat { get; set; } = null!;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
