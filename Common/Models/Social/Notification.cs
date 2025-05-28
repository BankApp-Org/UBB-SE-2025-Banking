using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models.Social
{
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationID { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        required public string Content { get; set; }

        [Required]
        required public int UserId { get; set; }

        [ForeignKey("UserId")]
        required public User User { get; set; }
    }
}
