namespace StockApp.ViewModels
{
    using Common.Models;
    using System.Collections.ObjectModel;

    /// <summary>
    /// ViewModel responsible for holding alert data for the UI.
    /// </summary>
    public class AlertViewModel : ViewModelBase
    {
        private string newAlertName = string.Empty;
        private string newAlertUpperBound = "0";
        private string newAlertLowerBound = "0";

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
        public string SelectedStockName { get; set; } = string.Empty;        /// <summary>
                                                                             /// Gets or sets the user-defined name for the new alert. Bound to a TextBox in the UI.
                                                                             /// </summary>
        public string NewAlertName
        {
            get => this.newAlertName;
            set => this.SetProperty(ref this.newAlertName, value);
        }

        /// <summary>
        /// Gets or sets the upper price boundary for the new alert. Bound to a TextBox in the UI.
        /// </summary>
        public string NewAlertUpperBound
        {
            get => this.newAlertUpperBound;
            set => this.SetProperty(ref this.newAlertUpperBound, value);
        }

        /// <summary>
        /// Gets or sets the lower price boundary for the new alert. Bound to a TextBox in the UI.
        /// </summary>
        public string NewAlertLowerBound
        {
            get => this.newAlertLowerBound;
            set => this.SetProperty(ref this.newAlertLowerBound, value);
        }
    }
}
