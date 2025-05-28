using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Common.Models.Trading
{    /// <summary>
     /// Represents the base information for a stock in the API.
     /// </summary>
    [Index(nameof(Name), IsUnique = true)]
    public class BaseStock
    {
        /// <summary>
        /// Gets or sets the unique identifier for the stock.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the display name of the stock.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the trading symbol of the stock.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the CNP identifier of the author who created this entry.
        /// </summary>
        [Required]
        [MaxLength(13)]
        public string AuthorCNP { get; set; } = string.Empty;

        [Required]
        public ICollection<FavoriteStock> Favorites { get; set; } = [];
    }
}