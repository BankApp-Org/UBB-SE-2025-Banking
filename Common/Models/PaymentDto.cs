namespace Common.Models
{
    using System.ComponentModel.DataAnnotations;

    public class PaymentDto
    {
        [Required]
        public decimal Penalty { get; set; } = 0;
    }
}