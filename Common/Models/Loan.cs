namespace Common.Models
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
        public string UserCnp { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal LoanAmount { get; set; }

        [Required]
        public DateTime ApplicationDate { get; set; }

        [Required]
        public DateTime RepaymentDate { get; set; }

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
        public string Status { get; set; }

        [Required]
        public int MonthlyPaymentsCompleted { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RepaidAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Penalty { get; set; }

        // Foreign key for the one-to-one relationship with LoanRequest
        public int LoanRequestId { get; set; }

        [Required]
        public LoanRequest LoanRequest { get; set; }

        public Loan() { }

        public Loan(int loanID, string userCnp, decimal loanAmount, DateTime applicationDate, DateTime repaymentDate, decimal interestRate, int numberOfMonths, decimal monthlyPaymentAmount, int monthlyPaymentsCompleted, decimal repaidAmount, decimal penalty, string status)
        {
            this.Id = loanID;
            this.UserCnp = userCnp;
            this.LoanAmount = loanAmount;
            this.ApplicationDate = applicationDate;
            this.RepaymentDate = repaymentDate;
            this.InterestRate = interestRate;
            this.NumberOfMonths = numberOfMonths;
            this.MonthlyPaymentAmount = monthlyPaymentAmount;
            this.Status = status;
            this.MonthlyPaymentsCompleted = monthlyPaymentsCompleted;
            this.RepaidAmount = repaidAmount;
            this.Penalty = penalty;
        }

        [NotMapped]
        public bool CanPay
        {
            get
            {
                return this.Status == "Approved" && this.RepaidAmount < this.LoanAmount;
            }
        }
    }

}
