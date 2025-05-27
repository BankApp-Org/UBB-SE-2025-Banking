namespace StockApp.ViewModels
{
    using Common.Models;
    using Common.Services;
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;

    public class LoanRequestViewModel(ILoanRequestService loanService)
    {
        private readonly ILoanRequestService _loanRequestService = loanService;

        public ObservableCollection<LoanRequest> LoanRequests { get; set; } = [];

        public bool IsLoading { get; private set; }

        public string ErrorMessage { get; private set; } = string.Empty;

        public async Task LoadLoanRequestsAsync()
        {
            this.IsLoading = true;
            this.ErrorMessage = string.Empty;
            this.LoanRequests.Clear();

            try
            {
                var requests = await this._loanRequestService.GetLoanRequests();
                foreach (var request in requests)
                {
                    this.LoanRequests.Add(request);
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessage = $"Failed to load loan requests: {ex.Message}";
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        public async Task<string> GetSuggestionAsync(LoanRequest loanRequest)
        {
            if (loanRequest == null)
            {
                return "Invalid loan request.";
            }

            try
            {
                return await this._loanRequestService.GiveSuggestion(loanRequest);
            }
            catch (Exception ex)
            {
                return $"Failed to get suggestion: {ex.Message}";
            }
        }
    }
}
