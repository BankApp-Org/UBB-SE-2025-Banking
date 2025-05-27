namespace StockApp.Views.Pages
{
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;
    using System;

    /// <summary>
    /// A page that displays transaction log data.
    /// </summary>
    public sealed partial class TransactionLogPage : Page
    {
        private readonly TransactionLogViewModel viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionLogPage"/> class.
        /// </summary>
        public TransactionLogPage(TransactionLogViewModel viewModel)
        {
            this.DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            this.InitializeComponent();
            this.viewModel = viewModel;
        }

        public void OnEndDateChanged(object sender, DatePickerSelectedValueChangedEventArgs e)
        {
            if (this.DataContext is TransactionLogViewModel viewModel)
            {
                if (sender is DatePicker datePicker && datePicker.SelectedDate.HasValue)
                {
                    viewModel.EndDate = datePicker.SelectedDate.Value.DateTime;
                }
            }
        }

        public void OnStartDateChanged(object sender, DatePickerSelectedValueChangedEventArgs e)
        {
            if (this.DataContext is TransactionLogViewModel viewModel)
            {
                if (sender is DatePicker datePicker && datePicker.SelectedDate.HasValue)
                {
                    viewModel.StartDate = datePicker.SelectedDate.Value.DateTime;
                }
            }
        }

        // Ensure the window is fully loaded before accessing Window.Current
        private void PageActivated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs e)
        {
            if (Window.Current != null)
            {
                System.Diagnostics.Debug.WriteLine("Page is now activated and Window.Current is available.");
            }
            else
            {
                throw new InvalidOperationException("Window.Current is null during page activation.");
            }
        }
    }
}
