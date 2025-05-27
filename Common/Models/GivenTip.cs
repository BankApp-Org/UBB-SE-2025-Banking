namespace Common.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a given tip that a user has received.
    /// </summary>
    public class GivenTip
    {
        /// <summary>
        /// Gets or sets the unique identifier for this given tip.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [Required]
        public int TipId { get; set; }

        [Required]

        public string UserCNP { get; set; }

        /// <summary>
        /// Gets or sets the CNP (personal identification number) of the user who received the tip.
        /// </summary>
        [Required]
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the date when the tip was given.
        /// </summary>
        public DateTime Date { get; set; } = DateTime.Now;

        /// <summary>
        /// Navigation property to the related Tip.
        /// </summary>
        public virtual Tip Tip { get; set; }
    }
}
