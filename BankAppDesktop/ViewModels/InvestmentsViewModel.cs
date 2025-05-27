namespace StockApp.ViewModels
{
    using Common.Models;
    using System.Collections.ObjectModel;

    /// <summary>
    /// ViewModel for investments display, containing only data properties.
    /// </summary>
    public class InvestmentsViewModel : ViewModelBase
    {
        private ObservableCollection<InvestmentPortfolio> usersPortfolio = [];
        private bool isLoading;
        private string errorMessage = string.Empty;

        /// <summary>
        /// Gets or sets the collection of user portfolios.
        /// </summary>
        public ObservableCollection<InvestmentPortfolio> UsersPortofolio
        {
            get => usersPortfolio;
            set => SetProperty(ref usersPortfolio, value);
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
        public InvestmentsViewModel()
        {
        }
    }
}
