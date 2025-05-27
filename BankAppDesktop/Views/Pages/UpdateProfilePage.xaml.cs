namespace StockApp.Views.Pages
{
    using Common.Models;
    using Common.Services;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media.Imaging;
    using StockApp.ViewModels;
    using System;
    using System.Threading.Tasks;

    public sealed partial class UpdateProfilePage : Page
    {
        private readonly UpdateProfilePageViewModel viewModelUpdate;
        private readonly IUserService userService;
        private readonly IAuthenticationService authService;
        private User? currentUser;

        public Page? PreviousPage { get; set; }

        public UpdateProfilePage(UpdateProfilePageViewModel viewModelUpdate, IUserService userService, IAuthenticationService authService)
        {
            this.InitializeComponent();
            this.viewModelUpdate = viewModelUpdate ?? throw new ArgumentNullException(nameof(viewModelUpdate));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.authService = authService ?? throw new ArgumentNullException(nameof(authService));
            this.DataContext = this.viewModelUpdate;

            this.Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            await LoadCurrentUserDataAsync();
        }

        private async Task LoadCurrentUserDataAsync()
        {
            try
            {
                viewModelUpdate.IsLoading = true;
                var session = authService.GetCurrentUserSession();
                if (session != null)
                {
                    currentUser = await userService.GetCurrentUserAsync();
                    if (currentUser != null)
                    {
                        viewModelUpdate.Username = currentUser.UserName ?? string.Empty;
                        viewModelUpdate.Image = currentUser.Image ?? string.Empty;
                        viewModelUpdate.Description = currentUser.Description ?? string.Empty;
                        viewModelUpdate.IsHidden = currentUser.IsHidden;
                        viewModelUpdate.IsAdmin = session.IsAdmin;

                        // Load image if available
                        if (!string.IsNullOrEmpty(currentUser.Image))
                        {
                            try
                            {
                                viewModelUpdate.ImageSource = new BitmapImage(new Uri(currentUser.Image));
                            }
                            catch
                            {
                                // Ignore image loading errors
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                viewModelUpdate.ErrorMessage = $"Error loading profile data: {ex.Message}";
                await ShowErrorDialog(viewModelUpdate.ErrorMessage);
            }
            finally
            {
                viewModelUpdate.IsLoading = false;
            }
        }

        public async void NavigateBack(object sender, RoutedEventArgs e)
        {
            if (this.PreviousPage != null)
            {
                App.MainAppWindow!.MainAppFrame.Content = this.PreviousPage;
            }
        }

        private async void GetAdminPassword(object sender, RoutedEventArgs e)
        {
            string userTryPass = this.PasswordTry.Text;

            try
            {
                // Simple admin password check - this should use proper authentication
                bool isAdmin = userTryPass == "admin123"; // This is just a placeholder
                viewModelUpdate.IsAdmin = isAdmin;

                string message = isAdmin ? "You are now ADMIN!" : "Incorrect Password!";
                string title = isAdmin ? "Success" : "Error";
                ContentDialog dialog = this.CreateDialog(title, message);
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Error checking admin password: {ex.Message}");
            }
        }

        private async void UpdateUserProfile(object sender, RoutedEventArgs e)
        {
            if (currentUser == null)
            {
                await ShowErrorDialog("No user profile loaded");
                return;
            }

            try
            {
                viewModelUpdate.IsLoading = true;
                viewModelUpdate.ErrorMessage = string.Empty;

                bool descriptionEmpty = this.MyDescriptionCheckBox?.IsChecked == true;
                bool newHidden = this.MyCheckBox?.IsChecked == true;
                string newUsername = this.UsernameInput?.Text ?? string.Empty;
                string newImage = this.ImageInput?.Text ?? string.Empty;
                string newDescription = this.DescriptionInput?.Text ?? string.Empty;

                // Validation
                if (string.IsNullOrEmpty(newUsername) && string.IsNullOrEmpty(newImage) && string.IsNullOrEmpty(newDescription)
                    && (newHidden == currentUser.IsHidden) && !descriptionEmpty)
                {
                    await this.ShowErrorDialog("Please fill up at least one of the information fields");
                    return;
                }

                if (!string.IsNullOrEmpty(newUsername) && (newUsername.Length < 8 || newUsername.Length > 24))
                {
                    await this.ShowErrorDialog("UserName must be 8-24 characters long.");
                    return;
                }

                if (!string.IsNullOrEmpty(newDescription) && newDescription.Length > 100)
                {
                    await this.ShowErrorDialog("The description should be max 100 characters long.");
                    return;
                }

                // Create updated user object
                var updatedUser = new User
                {
                    Id = currentUser.Id,
                    UserName = string.IsNullOrEmpty(newUsername) ? currentUser.UserName : newUsername,
                    Image = string.IsNullOrEmpty(newImage) ? currentUser.Image : newImage,
                    Description = descriptionEmpty ? string.Empty : (string.IsNullOrEmpty(newDescription) ? currentUser.Description : newDescription),
                    IsHidden = newHidden,
                    // Copy other existing properties
                    Email = currentUser.Email,
                    FirstName = currentUser.FirstName,
                    LastName = currentUser.LastName,
                    PhoneNumber = currentUser.PhoneNumber,
                    Birthday = currentUser.Birthday,
                    CNP = currentUser.CNP,
                    ZodiacSign = currentUser.ZodiacSign,
                    ZodiacAttribute = currentUser.ZodiacAttribute,
                    Balance = currentUser.Balance,
                    GemBalance = currentUser.GemBalance,
                    NumberOfOffenses = currentUser.NumberOfOffenses,
                    PasswordHash = currentUser.PasswordHash
                };

                await userService.UpdateUserAsync(updatedUser);
                // Refresh user data after update
                currentUser = await userService.GetCurrentUserAsync();
                if (currentUser != null)
                {
                    await this.ShowSuccessDialog("Profile updated successfully!");
                    // Update ViewModel with new data
                    viewModelUpdate.Username = currentUser.UserName ?? string.Empty;
                    viewModelUpdate.Image = currentUser.Image ?? string.Empty;
                    viewModelUpdate.Description = currentUser.Description ?? string.Empty;
                    viewModelUpdate.IsHidden = currentUser.IsHidden;
                }
                else
                {
                    await ShowErrorDialog("Failed to update profile. Please try again.");
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Error updating profile: {ex.Message}");
            }
            finally
            {
                viewModelUpdate.IsLoading = false;
            }
        }

        private async Task ShowErrorDialog(string message)
        {
            ContentDialog dialog = this.CreateDialog("Error", message);
            await dialog.ShowAsync();
        }

        private async Task ShowSuccessDialog(string message)
        {
            ContentDialog dialog = this.CreateDialog("Success", message);
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
    }
}
