using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models.Bank
{
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(13)]
        public string UserCnp { get; set; }

        [Required]
        [MaxLength(100)]
        public string ActivityName { get; set; }

        [Required]
        public int LastModifiedAmount { get; set; }
        public string ActivityDetails { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ActivityLog(int id, string userCNP, string name, int amount, string details)
        {
            Id = id;
            UserCnp = userCNP;
            ActivityName = name;
            LastModifiedAmount = amount;
            ActivityDetails = details;
        }

        public ActivityLog()
        {
            Id = 0;
            UserCnp = string.Empty;
            ActivityName = string.Empty;
            LastModifiedAmount = 0;
            ActivityDetails = string.Empty;
        }
    }
}
