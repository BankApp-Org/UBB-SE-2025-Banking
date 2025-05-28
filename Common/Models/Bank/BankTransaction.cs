using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models.Bank
{
    public class BankTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }
        [Required]
        required public string SenderIban { get; set; }

        [Required]
        required public string ReceiverIban { get; set; }

        [Required]
        [ForeignKey("SenderIban")]
        required public BankAccount SenderAccount { get; set; }

        [Required]
        [ForeignKey("ReceiverIban")]
        required public BankAccount ReceiverAccount { get; set; }

        [Required]
        public DateTime TransactionDatetime { get; set; }

        [Required]
        required public Currency SenderCurrency { get; set; }

        [Required]
        required public Currency ReceiverCurrency { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a positive value.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SenderAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a positive value.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ReceiverAmount { get; set; }

        [Required]
        required public string TransactionType { get; set; }

        [Required]
        [MaxLength(500)]
        required public string TransactionDescription { get; set; }
    }
}
