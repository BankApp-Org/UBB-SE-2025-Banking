using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models.Trading
{
    /// <summary>
    /// Represents the value of a stock at a specific time.
    /// </summary>
    public class StockValue
    {
        /// <summary>
        /// Gets or sets the unique identifier for the stock value.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the stock.
        /// </summary>
        [Required]
        [MaxLength(100)]
        required public string StockName { get; set; }        /// <summary>
                                                              /// Gets or sets the price of the stock.
                                                              /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        [Range(0, int.MaxValue)]
        required public decimal Price { get; set; }        /// <summary>
                                                           /// Gets or sets the date and time when the stock value was recorded.
                                                           /// </summary>
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        required public DateTime DateTime { get; set; }

        [ForeignKey("StockName")]
        public Stock Stock { get; set; } = null!;
    }
}