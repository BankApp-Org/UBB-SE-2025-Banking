using Common.Services;

namespace BankAppWeb.Views.LoanRequest
{
    using Common.Models.Bank;

    public class IndexModel
    {
        private readonly ILoanRequestService _loanRequestService;

        public IndexModel(ILoanRequestService loanRequestService)
        {
            _loanRequestService = loanRequestService;
        }

        public List<LoanRequest> Requests { get; private set; } = [];
        public Dictionary<int, string> Suggestions { get; private set; } = [];
        public string? ErrorMessage { get; set; } // Added setter

        public async Task OnGetAsync()
        {
            try
            {
                Requests = await _loanRequestService.GetUnsolvedLoanRequests();

                foreach (var request in Requests)
                {
                    var suggestion = await _loanRequestService.GiveSuggestion(request);
                    Suggestions[request.Id] = suggestion;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading loan requests: {ex.Message}";
            }
        }
    }
}