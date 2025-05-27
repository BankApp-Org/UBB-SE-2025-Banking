// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StockApp.Views.Pages
{
    using Common.Models;
    using Common.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Page for creating new user profiles.
    /// </summary>
    public sealed partial class CreateProfilePage : Page
    {
        private readonly CreateProfilePageViewModel viewModel;
        private readonly IUserService userService;
        private readonly IAuthenticationService authService;

        public CreateProfilePage(CreateProfilePageViewModel createProfilePageViewModel, IUserService userService, IAuthenticationService authService)
        {
            this.InitializeComponent();
            this.viewModel = createProfilePageViewModel ?? throw new ArgumentNullException(nameof(createProfilePageViewModel));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.authService = authService ?? throw new ArgumentNullException(nameof(authService));
            this.DataContext = this.viewModel;

            // Subscribe to ViewModel events
            this.viewModel.ProfileCreationRequested += OnProfileCreationRequested;
            this.viewModel.NavigateToLoginRequested += OnNavigateToLoginRequested;
        }

        private async void OnProfileCreationRequested(object? sender, User user)
        {
            await CreateProfileAsync(user);
        }

        private void OnNavigateToLoginRequested(object? sender, EventArgs e)
        {
            NavigateToLoginPage();
        }

        private async Task CreateProfileAsync(User user)
        {
            viewModel.IsLoading = true;
            viewModel.ErrorMessage = string.Empty;

            try
            {
                var createdUser = await userService.CreateUserAsync(user);
                if (createdUser != null)
                {
                    viewModel.SuccessMessage = "Profile created successfully!";
                    viewModel.ResetForm();

                    // Show success dialog
                    await ShowSuccessDialog("Profile created successfully! You can now log in with your credentials.");

                    // Navigate back to login
                    NavigateToLoginPage();
                }
                else
                {
                    viewModel.ErrorMessage = "Failed to create profile. Please try again.";
                }
            }
            catch (Exception ex)
            {
                viewModel.ErrorMessage = $"Error creating profile: {ex.Message}";
                await ShowErrorDialog(viewModel.ErrorMessage);
            }
            finally
            {
                viewModel.IsLoading = false;
            }
        }

        private void NavigateToLoginPage()
        {
            var loginPage = App.Host.Services.GetService<LoginPage>();
            if (loginPage != null)
            {
                App.MainAppWindow!.MainAppFrame.Content = loginPage;
            }
        }

        private async Task ShowErrorDialog(string message)
        {
            ContentDialog dialog = CreateDialog("Error", message);
            await dialog.ShowAsync();
        }

        private async Task ShowSuccessDialog(string message)
        {
            ContentDialog dialog = CreateDialog("Success", message);
            await dialog.ShowAsync();
        }

        private ContentDialog CreateDialog(string title, string message)
        {
            return new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot,
            };
        }

        private async void CreateProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!viewModel.ValidateInputs())
            {
                viewModel.ErrorMessage = "Please fill in all required fields.";
                await ShowErrorDialog(viewModel.ErrorMessage);
                return;
            }

            viewModel.RequestProfileCreation();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToLoginPage();
        }
    }
}
