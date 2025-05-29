namespace BankAppDesktop.Views.Pages
{
    using Common.Models.Trading;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using BankAppDesktop.ViewModels;
    using System;
    using System.Threading.Tasks;
    using Common.Services.Trading;

    public sealed partial class AlertsPage : Page
    {
        private readonly IAlertService alertService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertsPage"/> class.
        /// </summary>
        public AlertsPage(IAlertService alertService, AlertViewModel viewModel)
        {
            this.alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            this.ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            this.InitializeComponent();
            this.DataContext = this.ViewModel;

            // Load existing alerts into the collection
            this.LoadAlerts();
        }

        /// <summary>
        /// Gets or Sets the ViewModel for managing alerts.
        /// </summary>
        public AlertViewModel ViewModel { get; set; }

        public Page? PreviousPage { get; set; }

        private bool IsAlertValid()
        {
            return !string.IsNullOrWhiteSpace(this.ViewModel.NewAlertName) &&
                   decimal.TryParse(this.ViewModel.NewAlertUpperBound, out var upperBound) &&
                   decimal.TryParse(this.ViewModel.NewAlertLowerBound, out var lowerBound) &&
                   upperBound > lowerBound;
        }

        /// <summary>
        /// Creates a new alert with example data and shows a success or error dialog.
        /// </summary>
        private async Task CreateAlert()
        {
            if (!IsAlertValid())
            {
                await ShowMessageAsync("Error", "Please enter valid alert data.");
                return;
            }

            try
            {
                decimal upperBound = decimal.Parse(this.ViewModel.NewAlertUpperBound);
                decimal lowerBound = decimal.Parse(this.ViewModel.NewAlertLowerBound);

                // Create a new alert via the alertService and add it to the collection
                Alert newAlert = await this.alertService.CreateAlertAsync(
                    stockName: this.ViewModel.SelectedStockName,
                    name: this.ViewModel.NewAlertName,
                    upperBound: upperBound,
                    lowerBound: lowerBound,
                    toggleOnOff: true);
                this.ViewModel.Alerts.Add(newAlert);
                await ShowMessageAsync("Success", "Alert created successfully!");

                // Clear the form
                this.ViewModel.NewAlertName = string.Empty;
                this.ViewModel.NewAlertUpperBound = "0";
                this.ViewModel.NewAlertLowerBound = "0";
            }
            catch (Exception exception)
            {
                await ShowMessageAsync("Error", exception.Message);
            }
        }

        /// <summary>
        /// Saves all current alerts by validating and updating each via the alertService.
        /// </summary>
        private async Task SaveAlerts()
        {
            try
            {
                foreach (Alert alert in this.ViewModel.Alerts)
                {
                    if (alert.LowerBound >= alert.UpperBound)
                    {
                        throw new ArgumentException("Lower bound must be less than upper bound.");
                    }

                    if (string.IsNullOrWhiteSpace(alert.Name))
                    {
                        throw new ArgumentException("Alert name cannot be empty.");
                    }

                    await this.alertService.UpdateAlertAsync(
                        alert.AlertId,
                        alert.StockName,
                        alert.Name,
                        alert.UpperBound,
                        alert.LowerBound,
                        alert.ToggleOnOff);
                }

                await ShowMessageAsync("Success", "All alerts saved successfully!");
            }
            catch (Exception exception)
            {
                await ShowMessageAsync("Error", exception.Message);
            }
        }

        /// <summary>
        /// Deletes the specified alert after confirmation and shows a result dialog.
        /// </summary>
        /// <param name="alertToDelete">The alert object to delete.</param>
        private async Task DeleteAlert(Alert? alertToDelete)
        {
            if (alertToDelete != null)
            {
                await this.alertService.RemoveAlertAsync(alertToDelete.AlertId);
                this.ViewModel.Alerts.Remove(alertToDelete);
                await ShowMessageAsync("Success", "Alert deleted successfully!");
            }
            else
            {
                await ShowMessageAsync("Error", "Please select an alert to delete.");
            }
        }

        /// <summary>
        /// Loads all alerts from the alertService into the ViewModel's collection.
        /// </summary>
        private async void LoadAlerts()
        {
            this.ViewModel.Alerts.Clear();
            foreach (Alert alert in await this.alertService.GetAllAlertsAsync())
            {
                this.ViewModel.Alerts.Add(alert);
            }
        }

        /// <summary>
        /// Shows a message dialog with the specified title and message.
        /// </summary>
        private static async Task ShowMessageAsync(string title, string message)
        {
            ContentDialog dialog = new()
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = App.MainAppWindow!.MainAppFrame.XamlRoot,
            };
            await dialog.ShowAsync();
        }

        private async void CreateAlertButton_Click(object sender, RoutedEventArgs e)
        {
            await CreateAlert();
        }

        private async void SaveAlertsButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveAlerts();
        }

        private async void DeleteAlertButton_Click(object sender, RoutedEventArgs e)
        {
            if (AlertsListView.SelectedItem is Alert selectedAlert)
            {
                await DeleteAlert(selectedAlert);
            }
            else
            {
                await ShowMessageAsync("Error", "Please select an alert to delete.");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.PreviousPage != null)
            {
                App.MainAppWindow!.MainAppFrame.Content = this.PreviousPage;
            }
        }
    }
}
