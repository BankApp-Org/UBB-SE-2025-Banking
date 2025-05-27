namespace Common.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents a tip that can be given to a user based on their credit score bracket.
    /// </summary>
    public class Tip
    {
        /// <summary>
        /// Gets or sets the unique identifier for this tip.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the credit score bracket for this tip.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string CreditScoreBracket { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the tip
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the text of the tip.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string TipText { get; set; } = string.Empty;
    }
}
