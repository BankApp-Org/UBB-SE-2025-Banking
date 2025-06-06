namespace BankAppDesktop.ViewModels
{
    using Common.Models.Trading;
    using System.Collections.ObjectModel;

    /// <summary>
    /// ViewModel responsible for holding alert data for the UI.
    /// </summary>
    public class AlertViewModel : ViewModelBase
    {
        private string newAlertName = string.Empty;
        private string newAlertUpperBound = "0";
        private string newAlertLowerBound = "0";
        private bool alertValid = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertViewModel"/> class.
        /// </summary>
        public AlertViewModel()
        {
        }

        /// <summary>
        /// Gets the collection of alerts displayed in the UI.
        /// </summary>
        public ObservableCollection<Alert> Alerts { get; } = [];

        /// <summary>
        /// Gets or sets the selected stock for this viewmodel.
        /// </summary>
        public string SelectedStockName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user-defined name for the new alert. Bound to a TextBox in the UI.
        /// </summary>
        public string NewAlertName
        {
            get => this.newAlertName;
            set
            {
                if (this.SetProperty(ref this.newAlertName, value))
                {
                    this.AlertValid = this.IsAlertValid();
                }
            }
        }

        /// <summary>
        /// Gets or sets the upper price boundary for the new alert. Bound to a TextBox in the UI.
        /// </summary>
        public string NewAlertUpperBound
        {
            get => this.newAlertUpperBound;
            set
            {
                if (this.SetProperty(ref this.newAlertUpperBound, value))
                {
                    this.AlertValid = this.IsAlertValid();
                }
            }
        }

        /// <summary>
        /// Gets or sets the lower price boundary for the new alert. Bound to a TextBox in the UI.
        /// </summary>
        public string NewAlertLowerBound
        {
            get => this.newAlertLowerBound;
            set
            {
                if (this.SetProperty(ref this.newAlertLowerBound, value))
                {
                    this.AlertValid = this.IsAlertValid();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the alert to be inserted is valid.
        /// </summary>
        public bool AlertValid
        {
            get => this.alertValid;
            set => this.SetProperty(ref this.alertValid, value);
        }

        /// <summary>
        /// Validates whether the current alert input is valid.
        /// </summary>
        /// <returns>True if the alert is valid; otherwise, false.</returns>
        public bool IsAlertValid()
        {
            return !string.IsNullOrWhiteSpace(this.NewAlertName) &&
                   decimal.TryParse(this.NewAlertUpperBound, out var upperBound) &&
                   decimal.TryParse(this.NewAlertLowerBound, out var lowerBound) &&
                   upperBound > lowerBound;
        }
    }
}
