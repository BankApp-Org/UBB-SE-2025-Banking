using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models.Social
{
    public class Post
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostID { get; set; }

        [Required]
        required public string Title { get; set; }

        [Required]
        required public string Category { get; set; }

        [Required]
        required public string Content { get; set; }

        [Required]
        required public DateTime Timestamp { get; set; }
    }
}
