using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models.Social
{
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string MessageContent { get; set; } = string.Empty;

        public Message(int id, MessageType type, string message)
        {
            Id = id;
            Type = type;
            MessageContent = message;
        }

        public Message()
        {
            Id = 0;
            Type = MessageType.Text;
            MessageContent = string.Empty;
        }

        [Required]
        public int UserId { get; set; }

        [Required]
        public MessageType Type { get; set; }

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
