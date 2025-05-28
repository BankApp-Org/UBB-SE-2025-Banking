using Common.Models.Social;
using Common.Models.Trading;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Common.Models
{
    [Index(nameof(CNP), IsUnique = true)]
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
        [DefaultValue(0)]
        public int GemBalance { get; set; }

        [Range(0, int.MaxValue)]
        [DefaultValue(0)]
        public int NumberOfOffenses { get; set; }

        [Range(0, 100)]
        [DefaultValue(0)]
        public int RiskScore { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DefaultValue(0)]
        public decimal ROI { get; set; }

        [Range(0, 850)]
        [DefaultValue(0)]
        public int CreditScore { get; set; }

        [Required]
        public DateTime Birthday { get; set; }

        [MaxLength(20)]
        public string ZodiacSign { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ZodiacAttribute { get; set; } = string.Empty; [Range(0, int.MaxValue)]
        [DefaultValue(0)]
        public int NumberOfBillSharesPaid { get; set; }

        [Range(0, int.MaxValue)]
        [DefaultValue(0)]
        public int Income { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int ReportedCount { get; set; } = 0;

        public DateTime? TimeoutEnd { get; set; } = null;

        public List<User> Friends { get; set; } = [];

        public List<Chat> Chats { get; set; } = []; [Column(TypeName = "decimal(18,2)")]
        [DefaultValue(0)]
        public decimal Balance { get; set; }

        [JsonIgnore]
        [InverseProperty("User")]
        public ICollection<UserStock> OwnedStocks { get; set; } = [];
    }
}
