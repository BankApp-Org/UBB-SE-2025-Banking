namespace BankAppDesktop.Views.Pages
{
    using Common.Models.Bank;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using BankAppDesktop.ViewModels;
    using BankAppDesktop.Views.Components;
    using System;
    using System.Threading.Tasks;
    using Common.Services.Bank;
    using Common.Services;

    public sealed partial class LoansPage : Page
    {
        private readonly LoansViewModel viewModel;
        private readonly ILoanService loanService;
        private readonly IAuthenticationService authService;

        private TextBlock? noLoansMessage;
        private ContentDialog? contentDialog;
        private CreateLoanDialog? createLoanComponent;

        public LoansPage(LoansViewModel viewModel)
        {
            this.authService = App.Services.GetRequiredService<IAuthenticationService>();
            this.viewModel = viewModel;
            this.InitializeComponent();
            this.DataContext = viewModel;

            this.loanService = App.Host.Services.GetService<ILoanService>() ??
                throw new InvalidOperationException("LoanService not registered");

            // Find the NoLoansMessage TextBlock
            this.noLoansMessage = this.FindName("NoLoansMessage") as TextBlock;

            this.Loaded += LoansPage_Loaded;
            this.authService = authService;
        }

        private async void LoansPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLoansAsync();
        }

        private async Task LoadLoansAsync()
        {
            try
            {
                this.viewModel.IsLoading = true;

                // string userCNP = authService.GetCurrentUserSession()?.CNP ?? string.Empty;
                string userCNP = "1234567890123";
                var loans = await this.loanService.GetUserLoansAsync(userCNP); // Current user's loans
                this.viewModel.Loans.Clear();

                foreach (var loan in loans)
                {
                    this.viewModel.Loans.Add(loan);
                }

                // Update the no loans message visibility
                if (this.noLoansMessage != null)
                {
                    this.noLoansMessage.Visibility = this.viewModel.Loans.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                // Handle error - could show dialog or update ViewModel with error message
                System.Diagnostics.Debug.WriteLine($"Error loading loans: {ex.Message}");

                // Show error dialog to user
                var dialog = new ContentDialog
                {
                    XamlRoot = App.MainAppWindow!.MainAppFrame.XamlRoot,
                    Title = "Error",
                    Content = $"Failed to load loans: {ex.Message}",
                    CloseButtonText = "OK"
                };
                await dialog.ShowAsync();
            }
            finally
            {
                this.viewModel.IsLoading = false;
            }
        }

        private async void RequestNewLoanButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowCreateLoanDialogAsync();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadLoansAsync();
        }

        private async Task ShowCreateLoanDialogAsync()
        {
            try
            {
                // Create a new dialog instance each time to ensure clean state
                // Create the dialog content
                createLoanComponent = App.Host.Services.GetRequiredService<CreateLoanDialog>();
                createLoanComponent.LoanRequestSubmitted += CreateLoanComponent_LoanRequestSubmitted;
                createLoanComponent.DialogClosed += CreateLoanComponent_DialogClosed;

                // Create the dialog
                contentDialog = new ContentDialog
                {
                    XamlRoot = App.MainAppWindow!.MainAppFrame.XamlRoot,
                    Title = "Create Loan Request",
                    Content = createLoanComponent,
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    PrimaryButtonText = "Submit Request",
                    IsPrimaryButtonEnabled = true
                };

                contentDialog.PrimaryButtonClick += ContentDialog_PrimaryButtonClick;

                // Show the dialog
                await contentDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                // Handle error
                var errorDialog = new ContentDialog
                {
                    XamlRoot = App.MainAppWindow!.MainAppFrame.XamlRoot,
                    Title = "Error",
                    Content = $"Failed to open loan request dialog: {ex.Message}",
                    CloseButtonText = "OK"
                };
                await errorDialog.ShowAsync();
            }
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Prevent dialog from closing immediately for validation
            args.Cancel = true;

            // Handle submit in the component
            if (createLoanComponent != null && createLoanComponent.CanSubmitLoanRequest())
            {
                // Disable buttons during submission
                if (contentDialog != null)
                {
                    contentDialog.IsPrimaryButtonEnabled = false;
                    contentDialog.IsSecondaryButtonEnabled = false;
                    contentDialog.CloseButtonText = string.Empty; // Hide close button during submission
                }

                // Submit the request using the async method
                await createLoanComponent.SubmitLoanRequestAsync();
            }
        }

        private void CreateLoanComponent_DialogClosed(object? sender, EventArgs e)
        {
            // Close the dialog
            if (contentDialog != null)
            {
                contentDialog.Hide();

                // Clean up resources
                contentDialog.PrimaryButtonClick -= ContentDialog_PrimaryButtonClick;
                contentDialog = null;
            }

            if (createLoanComponent != null)
            {
                createLoanComponent.LoanRequestSubmitted -= CreateLoanComponent_LoanRequestSubmitted;
                createLoanComponent.DialogClosed -= CreateLoanComponent_DialogClosed;
                createLoanComponent = null;
            }
        }

        private void CreateLoanComponent_LoanRequestSubmitted(object? sender, LoanRequest e)
        {
            // Refresh the loans list
            _ = LoadLoansAsync();
        }

        private void LoanComponent_LoanUpdated(object sender, System.EventArgs e)
        {
            _ = LoadLoansAsync();
        }
    }
}
