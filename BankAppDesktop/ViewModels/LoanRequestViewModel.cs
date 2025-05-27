namespace StockApp.ViewModels
{
    using Common.Models;
    using System.Collections.ObjectModel;

    /// <summary>
    /// ViewModel for loan requests display, containing only data properties.
    /// </summary>
    public class LoanRequestViewModel : ViewModelBase
    {
        private ObservableCollection<LoanRequest> loanRequests = [];
        private bool isLoading;
        private string errorMessage = string.Empty;
        private string suggestion = string.Empty;

        /// <summary>
        /// Gets or sets the collection of loan requests.
        /// </summary>
        public ObservableCollection<LoanRequest> LoanRequests
        {
            get => loanRequests;
            set => SetProperty(ref loanRequests, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether data is being loaded.
        /// </summary>
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

        /// <summary>
        /// Gets or sets the current error message.
        /// </summary>
        public string ErrorMessage
        {
            get => errorMessage;
            set => SetProperty(ref errorMessage, value);
        }

        /// <summary>
        /// Gets or sets the suggestion text for loan requests.
        /// </summary>
        public string Suggestion
        {
            get => suggestion;
            set => SetProperty(ref suggestion, value);
        }
        public LoanRequestViewModel()
        {
        }
    }
}
