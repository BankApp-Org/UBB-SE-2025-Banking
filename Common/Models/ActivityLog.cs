using System.ComponentModel.DataAnnotations;

namespace Common.Models
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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ActivityLog(int id, string userCNP, string name, int amount, string details)
        {
            this.Id = id;
            this.UserCnp = userCNP;
            this.ActivityName = name;
            this.LastModifiedAmount = amount;
            this.ActivityDetails = details;
        }

        public ActivityLog()
        {
            this.Id = 0;
            this.UserCnp = string.Empty;
            this.ActivityName = string.Empty;
            this.LastModifiedAmount = 0;
            this.ActivityDetails = string.Empty;
        }
    }
}
