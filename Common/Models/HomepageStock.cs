namespace Common.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class HomepageStock
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Symbol { get; set; } = string.Empty;

        [Required]
        public Stock StockDetails { get; set; } = new Stock()
        {
            Name = string.Empty,
            Symbol = string.Empty,
            AuthorCNP = string.Empty,
            NewsArticles = [],
            Favorites = [],
            Price = 0,
            Quantity = 0
        };

        [NotMapped]
        public bool IsFavorite { get; set; }

        public decimal Change { get; set; }
    }
}
