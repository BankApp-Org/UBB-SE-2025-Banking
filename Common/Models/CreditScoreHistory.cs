using System.ComponentModel.DataAnnotations;

namespace Common.Models
{
    public class CreditScoreHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(13)]
        public string UserCnp { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Range(0, 1000)]
        public int Score { get; set; }

        public CreditScoreHistory()
        {
        }

        public CreditScoreHistory(int id, string userCnp, DateTime date, int creditScore)
        {
            Id = id;
            UserCnp = userCnp;
            Date = date;
            Score = creditScore;
        }
    }
}
