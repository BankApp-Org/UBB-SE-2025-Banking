namespace Common.Models
{
    using System.ComponentModel.DataAnnotations;
    public class ChatReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string SubmitterCnp { get; set; }

        [Required]
        [MaxLength(50)]
        required public string ReportedUserCnp { get; set; }

        [Required]
        [MaxLength(500)]
        required public string ReportedMessage { get; set; }
    }
}
