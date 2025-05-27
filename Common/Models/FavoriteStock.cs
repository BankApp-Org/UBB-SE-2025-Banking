namespace Common.Models
{
    using System.ComponentModel.DataAnnotations;
    /// <summary>
    /// Represents a favorite stock for a user.
    /// </summary>
    public class FavoriteStock
    {
        /// <summary>
        /// Gets or sets the unique identifier for the favorite stock.
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the CNP of the user who favorited the stock.
        /// </summary>
        [Required]
        public string UserCNP { get; set; }

        [Required]
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the name of the stock.
        /// </summary>
        [Required]
        public string StockName { get; set; }

        /// <summary>
        /// Gets or sets the stock associated with this favorite.
        /// </summary>
        [Required]
        public Stock Stock { get; set; }
    }
}