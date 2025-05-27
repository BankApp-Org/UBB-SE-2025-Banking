// Common/Models/Alert.cs
using System.ComponentModel.DataAnnotations;

namespace Common.Models
{
    using System.ComponentModel;

    /// <summary>
    /// Represents a stock alert with upper and lower price bounds and an on/off toggle.
    /// </summary>
    public class Alert
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the unique identifier for this alert.
        /// </summary>
        [Key]
        public int AlertId { get; set; }

        /// <summary>
        /// Gets or sets the stock symbol or name that this alert monitors.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string StockName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user-defined name for this alert.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the upper price boundary for triggering the alert.
        /// </summary>
        public decimal UpperBound { get; set; }

        /// <summary>
        /// Gets or sets the lower price boundary for triggering the alert.
        /// </summary>
        public decimal LowerBound { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the alert is active.
        /// </summary>
        public bool ToggleOnOff { get; set; }
    }
}
