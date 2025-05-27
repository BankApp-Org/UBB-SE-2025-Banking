namespace Common.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Investment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string InvestorCnp { get; set; }

        [MaxLength(500)]
        public string Details { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountInvested { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountReturned { get; set; }

        [Required]
        public DateTime InvestmentDate { get; set; }

        public Investment(int id, string investorCnp, string details, decimal amountInvested, decimal amountReturned, DateTime investmentDate)
        {
            this.Id = id;
            this.InvestorCnp = investorCnp;
            this.Details = details;
            this.AmountInvested = amountInvested;
            this.AmountReturned = amountReturned;
            this.InvestmentDate = investmentDate;
        }

        public Investment()
        {
            this.Id = 0;
            this.InvestorCnp = string.Empty;
            this.Details = string.Empty;
            this.AmountInvested = 0;
            this.AmountReturned = 0;
            this.InvestmentDate = DateTime.Now;
        }
    }
}