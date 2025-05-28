namespace Common.Models.Social
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    public class ChatReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(13)]
        required public string SubmitterCnp { get; set; }

        [Required]
        [ForeignKey("SubmitterCnp")]
        public User Submitter { get; set; } = null!;

        [Required]
        [MaxLength(13)]
        required public string ReportedUserCnp { get; set; }

        [Required]
        [ForeignKey("ReportedUserCnp")]
        public User ReportedUser { get; set; } = null!;

        [Required]
        required public int MessageId { get; set; }

        [Required]
        required public Message Message { get; set; }
    }
}
