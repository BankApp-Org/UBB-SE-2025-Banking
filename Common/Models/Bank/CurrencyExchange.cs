using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models.Bank
{
    public class CurrencyExchange
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        required public Currency FromCurrency { get; set; }

        [Required]
        required public Currency ToCurrency { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        required public decimal ExchangeRate { get; set; }
    }
}
