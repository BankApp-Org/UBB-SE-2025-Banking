namespace Common.Models.Bank
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
        [MaxLength(13)]
        public string InvestorCnp { get; set; }

        [Required]
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
            Id = id;
            InvestorCnp = investorCnp;
            Details = details;
            AmountInvested = amountInvested;
            AmountReturned = amountReturned;
            InvestmentDate = investmentDate;
        }

        public Investment()
        {
            Id = 0;
            InvestorCnp = string.Empty;
            Details = string.Empty;
            AmountInvested = 0;
            AmountReturned = 0;
            InvestmentDate = DateTime.Now;
        }
    }
}