
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models
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
        required public string StockName { get; set; }

        public Stock Stock { get; set; } = null!;

        /// <summary>
        /// Gets or sets the price of the stock.
        /// </summary>
        [Required]
        [Range(0, int.MaxValue)]
        required public int Price { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the stock value was recorded.
        /// </summary>
        [Required]
        required public DateTime DateTime { get; set; }
    }
}