namespace StockApp.ViewModels
{
    using Common.Models;
    using Common.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.Commands;
    using StockApp.Views.Pages;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public partial class CreateProfilePageViewModel : INotifyPropertyChanged
    {
        private readonly IUserService userService;

        private string image = string.Empty;
        private string username = string.Empty;
        private string description = string.Empty;
        private string firstName = string.Empty;
        private string lastName = string.Empty;
        private string email = string.Empty;
        private string phoneNumber = string.Empty;
        private DateTimeOffset birthday = DateTimeOffset.Now;
        private string cnp = string.Empty;
        private string zodiacSign = string.Empty;
        private string zodiacAttribute = string.Empty;
        private string password = string.Empty;

        public ICommand CreateProfileCommand { get; set; }

        public ICommand GoToLoginPageCommand { get; set; }

        public CreateProfilePageViewModel(IUserService userService)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.CreateProfileCommand = new RelayCommand(this.CreateProfile);
            this.GoToLoginPageCommand = new RelayCommand(GoToLoginPage);
        }

        private static void GoToLoginPage(object? parameter = null)
        {
            App.MainAppWindow!.MainAppFrame.Content = App.Host.Services.GetService<LoginPage>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Image
        {
            get => this.image;
            set => this.SetProperty(ref this.image, value);
        }

        public string Username
        {
            get => this.username;
            set => this.SetProperty(ref this.username, value);
        }

        public string Description
        {
            get => this.description;
            set => this.SetProperty(ref this.description, value);
        }

        public string FirstName
        {
            get => this.firstName;
            set => this.SetProperty(ref this.firstName, value);
        }

        public string LastName
        {
            get => this.lastName;
            set => this.SetProperty(ref this.lastName, value);
        }

        public string Email
        {
            get => this.email;
            set => this.SetProperty(ref this.email, value);
        }

        public string PhoneNumber
        {
            get => this.phoneNumber;
            set => this.SetProperty(ref this.phoneNumber, value);
        }

        public DateTimeOffset Birthday
        {
            get => this.birthday;
            set => this.SetProperty(ref this.birthday, value);
        }

        public string CNP
        {
            get => this.cnp;
            set => this.SetProperty(ref this.cnp, value);
        }

        public string ZodiacSign
        {
            get => this.zodiacSign;
            set => this.SetProperty(ref this.zodiacSign, value);
        }

        public string ZodiacAttribute
        {
            get => this.zodiacAttribute;
            set => this.SetProperty(ref this.zodiacAttribute, value);
        }

        public string Password
        {
            get => this.password;
            set => this.SetProperty(ref this.password, value);
        }

        private async void CreateProfile(object o)
        {
            User user = new()
            {
                Image = this.Image,
                UserName = this.Username,
                Description = this.Description,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,
                PhoneNumber = this.PhoneNumber,
                Birthday = this.Birthday.DateTime,
                CNP = this.CNP,
                ZodiacSign = this.ZodiacSign,
                ZodiacAttribute = this.ZodiacAttribute,
                Balance = 0,
                IsHidden = false,
                GemBalance = 0,
                NumberOfOffenses = 0,
                PasswordHash = this.Password,
            };

            try
            {
                await this.userService.CreateUser(user);
                await ShowSuccessDialog("Profile created successfully!");

                GoToLoginPage();
            }
            catch (Exception ex)
            {
                await ShowErrorDialog(ex.Message);
            }
        }

        private static async Task ShowErrorDialog(string message)
        {
            if (App.MainAppWindow == null)
            {
                throw new InvalidOperationException("Main window not found");
            }

            ContentDialog dialog = new()
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = App.MainAppWindow.MainAppFrame.XamlRoot,
            };
            await dialog.ShowAsync();
        }

        private static async Task ShowSuccessDialog(string message)
        {
            if (App.MainAppWindow == null)
            {
                throw new InvalidOperationException("Main window not found");
            }

            ContentDialog dialog = new()
            {
                Title = "Success",
                Content = message,
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = App.MainAppWindow.MainAppFrame.XamlRoot,
            };
            await dialog.ShowAsync();
        }

        private void SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
