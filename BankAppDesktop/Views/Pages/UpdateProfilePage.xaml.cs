namespace StockApp.Views.Pages
{
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;
    using System;
    using System.Threading.Tasks;

    public sealed partial class UpdateProfilePage : Page
    {
        private readonly UpdateProfilePageViewModel viewModelUpdate;

        public Page? PreviousPage { get; set; }

        public UpdateProfilePage(UpdateProfilePageViewModel viewModelUpdate)
        {
            this.InitializeComponent();
            this.viewModelUpdate = viewModelUpdate ?? throw new ArgumentNullException(nameof(viewModelUpdate));
        }

        public async void NavigateBack(object sender, RoutedEventArgs e)
        {
            if (this.PreviousPage != null)
            {
                if (this.PreviousPage is ProfilePage profile)
                {
                    await profile.ViewModel.LoadProfileData();
                }

                App.MainAppWindow!.MainAppFrame.Content = this.PreviousPage;
            }
        }

        private async void GetAdminPassword(object sender, RoutedEventArgs e)
        {
            string userTryPass = this.PasswordTry.Text;

            // TODO: holy shit this code is unholy do actual auth checking
            bool isAdmin = false;
            await this.viewModelUpdate.UpdateAdminModeAsync(isAdmin);

            string message = isAdmin ? "You are now ADMIN!" : "Incorrect Password!";
            string title = isAdmin ? "Success" : "Error";
            ContentDialog dialog = this.CreateDialog(title, message);
            await dialog.ShowAsync();
        }

        private async void UpdateUserProfile(object sender, RoutedEventArgs e)
        {
            if (this.viewModelUpdate == null)
            {
                throw new InvalidOperationException("ViewModel is not initialized");
            }

            bool descriptionEmpty = this.MyDescriptionCheckBox?.IsChecked == true;
            bool newHidden = this.MyCheckBox?.IsChecked == true;
            string newUsername = this.UsernameInput?.Text ?? string.Empty;
            string newImage = this.ImageInput?.Text ?? string.Empty;
            string newDescription = this.DescriptionInput?.Text ?? string.Empty;

            if (string.IsNullOrEmpty(newUsername) && string.IsNullOrEmpty(newImage) && string.IsNullOrEmpty(newDescription)
                && (this.MyCheckBox?.IsChecked == false && await this.viewModelUpdate.IsHidden() == false) && this.MyDescriptionCheckBox?.IsChecked == false)
            {
                await this.ShowErrorDialog("Please fill up at least one of the information fields");
                return;
            }

            if ((newUsername.Length < 8 || newUsername.Length > 24) && newUsername.Length != 0)
            {
                await this.ShowErrorDialog("UserName must be 8-24 characters long.");
                return;
            }

            if (newDescription.Length > 100)
            {
                await this.ShowErrorDialog("The description should be max 100 characters long.");
                return;
            }

            if (string.IsNullOrEmpty(newUsername))
            {
                newUsername = await this.viewModelUpdate.GetUsername() ?? throw new InvalidOperationException("UserName cannot be null");
            }

            if (descriptionEmpty)
            {
                newDescription = string.Empty;
            }
            else if (string.IsNullOrWhiteSpace(newDescription))
            {
                newDescription = await this.viewModelUpdate.GetDescription() ?? string.Empty;
            }

            if (string.IsNullOrEmpty(newImage))
            {
                newImage = await this.viewModelUpdate.GetImage() ?? throw new InvalidOperationException("Image cannot be null");
            }

            await this.viewModelUpdate.UpdateAllAsync(newUsername, newImage, newDescription, newHidden);
            await this.ShowSuccessDialog("Profile updated successfully!");
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
