namespace Common.Models.Trading
{
    using Microsoft.EntityFrameworkCore;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a favorite stock for a user.
    /// </summary>
    [PrimaryKey(nameof(UserCNP), nameof(StockName))]
    public class FavoriteStock
    {
        /// <summary>
        /// Gets or sets the CNP of the user who favorited the stock.
        /// </summary>
        [Required]
        [MaxLength(13)]
        public string UserCNP { get; set; } = string.Empty;

        [Required]
        [ForeignKey("UserCNP")]
        public User User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the name of the stock.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string StockName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the stock associated with this favorite.
        /// </summary>
        [Required]
        [ForeignKey("StockName")]
        public Stock Stock { get; set; } = null!;
    }
}