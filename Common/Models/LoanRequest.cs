using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models
{
    public class LoanRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(13)]
        public required string UserCnp { get; set; }

        [Required]
        public required Loan Loan { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Status { get; set; }

    }
}
