using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models.Bank
{
    public class BankAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        required public string Iban { get; set; }

        [Required]
        required public Currency Currency { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Balance must be a positive value.")]
        public decimal Balance { get; set; }

        [Required]
        required public bool Blocked { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        required public string Name { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Daily limit must be a positive value.")]
        public decimal DailyLimit { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Maximum per transaction must be a positive value.")]
        public decimal MaximumPerTransaction { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Maximum number of transactions must be at least 1.")]
        public int MaximumNrTransactions { get; set; }

        [Required]
        required public List<BankTransaction> Transactions { get; set; } = [];

        [Required]
        required public User User { get; set; }
    }
}
