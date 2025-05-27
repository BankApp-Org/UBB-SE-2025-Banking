using System.ComponentModel.DataAnnotations;

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
        required public decimal BillShare { get; set; }
    }
}