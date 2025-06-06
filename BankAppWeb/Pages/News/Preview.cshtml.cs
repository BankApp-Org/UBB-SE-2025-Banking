using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BankAppWeb.Pages.News
{
    public class PreviewModel : PageModel
    {
        public string Title { get; private set; }
        public string Summary { get; private set; }
        public string Content { get; private set; }
        public string Topic { get; private set; }
        public string Source { get; private set; } = "User Generated";
        public DateTime PublishedDate { get; private set; } = DateTime.Now;
        public List<string> RelatedStocks { get; private set; } = [];
        public bool IsPreview { get; private set; }

        public IActionResult OnGet()
        {
            // Check if this is a preview from TempData
            IsPreview = TempData["IsPreview"] as bool? ?? false;

            if (!IsPreview)
            {
                return RedirectToPage("/News/Create");
            }

            // Get preview data from TempData
            Title = TempData["PreviewTitle"] as string ?? "Sample Article Title";
            Summary = TempData["PreviewSummary"] as string ?? "";
            Content = TempData["PreviewContent"] as string ?? "No content provided.";
            Topic = TempData["PreviewTopic"] as string ?? "General";

            // Parse related stocks
            var relatedStocksText = TempData["PreviewRelatedStocks"] as string;
            if (!string.IsNullOrEmpty(relatedStocksText))
            {
                RelatedStocks = [.. relatedStocksText
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())];
            }

            // Preserve the TempData for the edit action
            TempData.Keep();

            return Page();
        }

        public IActionResult OnPostEdit()
        {
            // Return to the create page while preserving all input
            return RedirectToPage("/News/Create");
        }

        public IActionResult OnPostSubmit()
        {
            // The actual submission logic is in the Create page
            // This is just a convenience action to submit directly from preview
            return RedirectToPage("/News/Create");
        }
    }
}