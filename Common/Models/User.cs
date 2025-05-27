using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Common.Models
{
    public class User : IdentityUser<int>
    {
        [Required]
        [MaxLength(13)]
        public string CNP { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Image { get; set; } = string.Empty;

        public bool IsHidden { get; set; }

        [Range(0, int.MaxValue)]
        public int GemBalance { get; set; }

        [Range(0, int.MaxValue)]
        public int NumberOfOffenses { get; set; }

        [Range(0, 100)]
        public int RiskScore { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ROI { get; set; }

        [Range(0, 850)]
        public int CreditScore { get; set; }

        [Required]
        public DateTime Birthday { get; set; }

        [MaxLength(20)]
        public string ZodiacSign { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ZodiacAttribute { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int NumberOfBillSharesPaid { get; set; }

        [Range(0, int.MaxValue)]
        public int Income { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        [JsonIgnore]
        public ICollection<UserStock> OwnedStocks { get; set; } = [];
    }
}
