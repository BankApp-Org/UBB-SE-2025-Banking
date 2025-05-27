namespace Common.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a news article about stocks, including metadata and content.
    /// </summary>
    public class NewsArticle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewsArticle"/> class with specified details.
        /// </summary>
        /// <param name="articleId">Unique identifier of the article.</param>
        /// <param name="title">Title of the article.</param>
        /// <param name="summary">Short summary of the article.</param>
        /// <param name="content">Full content of the article.</param>
        /// <param name="source">Source from which the article was retrieved.</param>
        /// <param name="publishedDate">Publication date of the article.</param>
        /// <param name="relatedStocks">Collection of related stock symbols or names.</param>
        /// <param name="status">Current status of the article (e.g., Pending, Published).</param>
        public NewsArticle(
            string articleId,
            string title,
            string summary,
            string content,
            string source,
            string topic,
            DateTime publishedDate,
            IEnumerable<Stock> relatedStocks,
            Status status = Status.Pending)
        {
            this.ArticleId = articleId;
            this.Title = title;
            this.Summary = summary;
            this.Content = content;
            this.Source = source;
            this.PublishedDate = publishedDate;
            this.Topic = topic;
            // Copy the incoming sequence to a List for mutability and indexing
            this.RelatedStocks = [.. relatedStocks];
            this.Status = status;
        }
        public NewsArticle()
        {
            this.RelatedStocks = [];
        }

        /// <summary>
        /// Gets or sets the unique identifier of the article.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ArticleId { get; set; }

        /// <summary>
        /// Gets or sets the title of the article.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a short summary of the article.
        /// </summary>
        [MaxLength(500)]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the full content of the article.
        /// </summary>
        [Required]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the source of the article.
        /// </summary>
        [MaxLength(100)]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the publication date of the article.
        /// </summary>
        [Required]
        public DateTime PublishedDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the article has been read.
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this article relates to the watchlist.
        /// </summary>
        public bool IsWatchlistRelated { get; set; }

        /// <summary>
        /// Gets or sets the category of the article.
        /// </summary>
        [MaxLength(50)]
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the list of related stock symbols or names.
        /// </summary>
        public List<Stock> RelatedStocks { get; set; }

        /// <summary>
        /// Gets or sets the current status of the article.
        /// </summary>
        [Required]
        public Status Status { get; set; }

        [Required]
        public string AuthorCNP { get; set; } = string.Empty;

        [Required]
        public User Author { get; set; } = null!;

        [Required]
        public string Topic { get; set; } = string.Empty;
    }
}
