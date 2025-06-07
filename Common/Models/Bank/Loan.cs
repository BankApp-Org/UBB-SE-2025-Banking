namespace Common.Models.Bank
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Loan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(13)]
        public string UserCnp { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal LoanAmount { get; set; }

        [Required]
        public Currency Currency { get; set; } = Currency.USD;

        // Add disbursement account field
        [MaxLength(50)]
        public string? DisbursementAccountIban { get; set; }

        [Required]
        public DateTime ApplicationDate { get; set; }

        [Required]
        public DateTime RepaymentDate { get; set; }

        [Required]
        public DateTime DeadlineDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InterestRate { get; set; }

        [Required]
        public int NumberOfMonths { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyPaymentAmount { get; set; }

        [Required]
        [MaxLength(100)]
        public string Status { get; set; } = string.Empty;

        [Required]
        public int MonthlyPaymentsCompleted { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RepaidAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxPercentage { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Penalty { get; set; }

        // Foreign key for the one-to-one relationship with LoanRequest
        [ForeignKey("LoanRequest")]
        public int LoanRequestId { get; set; }

        [Required]
        [InverseProperty("Loan")]
        public LoanRequest LoanRequest { get; set; } = null!;

        public Loan() { }

        public Loan(int loanID, string userCnp, decimal loanAmount, DateTime applicationDate, DateTime repaymentDate, decimal interestRate, int numberOfMonths, decimal monthlyPaymentAmount, int monthlyPaymentsCompleted, decimal repaidAmount, decimal penalty, string status)
        {
            Id = loanID;
            UserCnp = userCnp;
            LoanAmount = loanAmount;
            ApplicationDate = applicationDate;
            RepaymentDate = repaymentDate;
            InterestRate = interestRate;
            NumberOfMonths = numberOfMonths;
            MonthlyPaymentAmount = monthlyPaymentAmount;
            Status = status;
            MonthlyPaymentsCompleted = monthlyPaymentsCompleted;
            RepaidAmount = repaidAmount;
            Penalty = penalty;
        }

        [NotMapped]
        public bool CanPay
        {
            get
            {
                return Status == "Approved" && RepaidAmount < LoanAmount;
            }
        }

        [NotMapped]
        public decimal AmountToPay => this.LoanAmount * (1 + (this.TaxPercentage / 100));
    }

}
