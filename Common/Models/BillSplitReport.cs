using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models
{
    using System;

    public class BillSplitReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(13)]
        required public string ReportedUserCnp { get; set; }

        [Required]
        [StringLength(13)]
        required public string ReportingUserCnp { get; set; }

        [Required]
        required public DateTime DateOfTransaction { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        required public decimal BillShare { get; set; }
    }
}