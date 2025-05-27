namespace StockApp.Views.Pages
{
    using Common.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;
    using StockApp.Views.Components;
    using System;

    public sealed partial class LoansPage : Page
    {
        private readonly LoansViewModel viewModel;
        private readonly ILoanService loanService;
        private TextBlock? noLoansMessage;
        private ContentDialog? contentDialog;
        private CreateLoanDialog? createLoanComponent;

        public LoansPage(LoansViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.InitializeComponent();
            this.DataContext = viewModel;

            this.loanService = App.Host.Services.GetService<ILoanService>() ??
                throw new InvalidOperationException("LoanService not registered");

            // Find the NoLoansMessage TextBlock
            this.noLoansMessage = this.FindName("NoLoansMessage") as TextBlock;

            // Subscribe to ViewModel events
            this.viewModel.ShowCreateLoanDialog += ViewModel_ShowCreateLoanDialog;
            this.viewModel.LoansUpdated += ViewModel_LoansUpdated;

            this.Loaded += (s, e) =>
            {
                if (viewModel.LoadLoansCommand.CanExecute(null))
                {
                    viewModel.LoadLoansCommand.Execute(null);
                }
            };
        }

        private void ViewModel_LoansUpdated(object? sender, EventArgs e)
        {
            // Show or hide the no loans message based on the count
            if (this.noLoansMessage != null)
            {
                this.noLoansMessage.Visibility = viewModel.Loans.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private async void ViewModel_ShowCreateLoanDialog(object? sender, EventArgs e)
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

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
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

                // Submit the request
                createLoanComponent.SubmitLoanRequest();
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

        private void CreateLoanComponent_LoanRequestSubmitted(object? sender, Common.Models.LoanRequest e)
        {
            // Refresh the loans list
            this.viewModel.OnLoansUpdatedCommand.Execute(null);
        }

        private void LoanComponent_LoanUpdated(object sender, System.EventArgs e)
        {
            this.viewModel.OnLoansUpdatedCommand.Execute(null);
        }
    }
}
