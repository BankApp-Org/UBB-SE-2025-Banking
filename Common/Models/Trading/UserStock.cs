using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Common.Models.Trading
{    /// <summary>
     /// Represents a user's stock holding, including its base information and quantity.
     /// </summary>
     /// <param name="name">The display name of the stock.</param>
     /// <param name="symbol">The trading symbol of the stock.</param>
     /// <param name="authorCnp">The CNP identifier of the author who created this entry.</param>
     /// <param name="quantity">The number of shares held by the user.</param>
    [PrimaryKey(nameof(UserCnp), nameof(StockName))]
    public class UserStock
    {
        [Required]
        [MaxLength(13)]
        public string UserCnp { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string StockName { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; }

        [ForeignKey("StockName")]
        public Stock Stock { get; set; } = null!;

        [JsonIgnore]
        [ForeignKey("UserCnp")]
        public User User { get; set; } = null!;
    }
}
