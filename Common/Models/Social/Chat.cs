using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models.Social
{
    public class Chat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        required public string ChatName { get; set; }

        [Required]
        required public List<User> Users { get; set; } = [];

        [Required]
        required public List<Message> Messages { get; set; } = [];

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public override string ToString()
        {
            return ChatName;
        }
    }
}
