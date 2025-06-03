using BankAppDesktop.ViewModels;
using Common.Services;
using Common.Services.Social;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;

namespace BankAppDesktop.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TipsPage : Page
    {
        private readonly IMessageService _messagesService;
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;
        private TipHistoryViewModel _viewModel;

        public bool IsAdmin => _authenticationService.IsUserAdmin();

        public bool IsLoggedIn => _authenticationService.IsUserLoggedIn();

        public TipsPage(IMessageService messagesService, IUserService userService, TipHistoryViewModel viewModel, IAuthenticationService authenticationService)
        {
            this.InitializeComponent();
            _messagesService = messagesService ?? throw new ArgumentNullException(nameof(messagesService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            this.DataContext = _viewModel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await LoadMessagesAsync();
        }

        private async Task LoadMessagesAsync()
        {
            try
            {
                _viewModel.IsLoading = true;

                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                {
                    ShowError("User not found. Please log in again.");
                    return;
                }

                var messages = await _messagesService.GetMessagesForUserAsync(user.CNP);
                _viewModel.MessageHistory.Clear();
                foreach (var message in messages)
                {
                    _viewModel.MessageHistory.Add(message);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading messages: {ex.Message}");
            }
            finally
            {
                _viewModel.IsLoading = false;
            }
        }

        private void AddTipButton_Click(object sender, RoutedEventArgs e)
        {
            TipTypeTextBox.Text = string.Empty;
            TipMessageTextBox.Text = string.Empty;
            _ = AddTipDialog.ShowAsync();
        }

        private async void AddTipDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true; // Prevent automatic dialog closing

            var type = TipTypeTextBox.Text?.Trim();
            var message = TipMessageTextBox.Text?.Trim();

            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(message))
            {
                ShowError("Please fill in all fields.");
                return;
            }

            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                {
                    ShowError("User not found. Please log in again.");
                    return;
                }

                throw new NotImplementedException("This should not exist");
                // await _messagesService.GiveMessageToUserAsync(user.CNP, type, message);
                ShowSuccess("Tip added successfully!");
                AddTipDialog.Hide();
                await LoadMessagesAsync();
            }
            catch (Exception ex)
            {
                ShowError($"Error adding tip: {ex.Message}");
            }
        }

        private void AddTipDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Cancel button clicked - dialog will close automatically
        }

        private void ShowError(string message)
        {
            ErrorInfoBar.Message = message;
            ErrorInfoBar.IsOpen = true;
            SuccessInfoBar.IsOpen = false;
        }

        private void ShowSuccess(string message)
        {
            SuccessInfoBar.Message = message;
            SuccessInfoBar.IsOpen = true;
            ErrorInfoBar.IsOpen = false;
        }
    }
}
