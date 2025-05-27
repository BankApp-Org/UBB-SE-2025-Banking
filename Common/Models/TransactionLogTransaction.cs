namespace Common.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a single transaction entry in the transaction log,
    /// containing details such as stock symbol, type, amount, and author.
    /// </summary>
    public class TransactionLogTransaction
    {
        /// <summary>
        /// Gets or sets the unique identifier for the transaction.
        /// </summary>
        [Key]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the trading symbol of the stock involved in the transaction.
        /// </summary>
        [Required]
        [MaxLength(10)]
        required public string StockSymbol { get; set; }

        /// <summary>
        /// Gets or sets the display name of the stock involved in the transaction.
        /// </summary>
        [Required]
        [MaxLength(100)]
        required public string StockName { get; set; }

        /// <summary>
        /// Gets or sets the type of transaction ("BUY" or "SELL").
        /// </summary>
        [Required]
        [MaxLength(4)]
        required public string Type { get; set; }

        /// <summary>
        /// Gets or sets the number of shares involved in the transaction.
        /// </summary>
        [Range(1, int.MaxValue)]
        required public int Amount { get; set; }

        /// <summary>
        /// Gets or sets the price paid per share in the transaction.
        /// </summary>
        [Range(1, int.MaxValue)]
        required public int PricePerStock { get; set; }

        /// <summary>
        /// Gets the total value of the transaction (Amount multiplied by PricePerStock).
        /// </summary>
        [NotMapped]
        public int TotalValue => this.Amount * this.PricePerStock;

        /// <summary>
        /// Gets or sets the date and time when the transaction took place.
        /// </summary>
        [Required]
        required public DateTime Date { get; set; }

        [Required]
        required public string AuthorCNP { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the transaction.
        /// </summary>
        [Required]
        required public User Author { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionLogTransaction"/> class.
        /// </summary>
        public TransactionLogTransaction()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionLogTransaction"/> class with parameters.
        /// </summary>
        /// <param name="stockSymbol">The trading symbol of the stock; cannot be null or whitespace.</param>
        /// <param name="stockName">The display name of the stock; cannot be null or whitespace.</param>
        /// <param name="type">The transaction type, must be "BUY" or "SELL".</param>
        /// <param name="amount">The number of shares; must be greater than zero.</param>
        /// <param name="pricePerStock">The price per share; must be greater than zero.</param>
        /// <param name="date">The date and time when the transaction occurred.</param>
        /// <param name="author">The user who performed the transaction; cannot be null.</param>
        public TransactionLogTransaction(
            string stockSymbol,
            string stockName,
            string type,
            int amount,
            int pricePerStock,
            DateTime date,
            User author)
        {
            if (string.IsNullOrWhiteSpace(stockSymbol))
            {
                throw new ArgumentException("StockSymbol required");
            }

            if (string.IsNullOrWhiteSpace(stockName))
            {
                throw new ArgumentException("StockName required");
            }

            if (type is not ("BUY" or "SELL"))
            {
                throw new ArgumentException("Type must be BUY or SELL");
            }

            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0");
            }

            if (pricePerStock <= 0)
            {
                throw new ArgumentException("PricePerStock must be greater than 0");
            }

            if (author == null)
            {
                throw new ArgumentException("Author required");
            }

            this.StockSymbol = stockSymbol;
            this.StockName = stockName;
            this.Type = type;
            this.Amount = amount;
            this.PricePerStock = pricePerStock;
            this.Date = date;
            this.Author = author;
        }
    }
}
