using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Common.Services.Social;

namespace BankAppWeb.Models
{
    public class ReportViewModel
    {
        public int ChatId { get; set; }

        public int MessageId { get; set; }

        public int ReportedUserId { get; set; }

        [Required(ErrorMessage = "Please select a report category.")]
        public ReportReason SelectedReportReason { get; set; }

        [StringLength(500, ErrorMessage = "The reason cannot be longer than 500 characters.")]
        public string OtherReason { get; set; } = string.Empty;

        /// <summary>
        /// Gets all available report reasons from the enum.
        /// </summary>
        public List<ReportReason> AvailableReportReasons =>
            System.Enum.GetValues<ReportReason>().ToList();
    }
}