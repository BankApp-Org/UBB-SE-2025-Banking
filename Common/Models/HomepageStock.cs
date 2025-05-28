namespace Common.Models
{
    using Common.Models.Trading;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class HomepageStock
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(10)]
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
        }; [NotMapped]
        public bool IsFavorite { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Change { get; set; }
    }
}
