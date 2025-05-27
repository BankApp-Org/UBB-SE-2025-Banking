namespace StockApp.ViewModels
{
    using Common.Models;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// ViewModel for the create profile page containing only data properties.
    /// Business logic for profile creation is handled in code-behind.
    /// </summary>
    public partial class CreateProfilePageViewModel : INotifyPropertyChanged
    {
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
        private bool isLoading = false;
        private string errorMessage = string.Empty;
        private string successMessage = string.Empty;

        /// <summary>
        /// Event raised when a profile creation is requested.
        /// </summary>
        public event EventHandler<User>? ProfileCreationRequested;

        /// <summary>
        /// Event raised when navigation to login page is requested.
        /// </summary>
        public event EventHandler? NavigateToLoginRequested;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateProfilePageViewModel"/> class.
        /// </summary>
        public CreateProfilePageViewModel()
        {
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the user's profile image.
        /// </summary>
        public string Image
        {
            get => this.image;
            set => this.SetProperty(ref this.image, value);
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username
        {
            get => this.username;
            set => this.SetProperty(ref this.username, value);
        }

        /// <summary>
        /// Gets or sets the user's description.
        /// </summary>
        public string Description
        {
            get => this.description;
            set => this.SetProperty(ref this.description, value);
        }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName
        {
            get => this.firstName;
            set => this.SetProperty(ref this.firstName, value);
        }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName
        {
            get => this.lastName;
            set => this.SetProperty(ref this.lastName, value);
        }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        public string Email
        {
            get => this.email;
            set => this.SetProperty(ref this.email, value);
        }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        public string PhoneNumber
        {
            get => this.phoneNumber;
            set => this.SetProperty(ref this.phoneNumber, value);
        }

        /// <summary>
        /// Gets or sets the birthday.
        /// </summary>
        public DateTimeOffset Birthday
        {
            get => this.birthday;
            set => this.SetProperty(ref this.birthday, value);
        }

        /// <summary>
        /// Gets or sets the CNP (personal identification number).
        /// </summary>
        public string CNP
        {
            get => this.cnp;
            set => this.SetProperty(ref this.cnp, value);
        }

        /// <summary>
        /// Gets or sets the zodiac sign.
        /// </summary>
        public string ZodiacSign
        {
            get => this.zodiacSign;
            set => this.SetProperty(ref this.zodiacSign, value);
        }

        /// <summary>
        /// Gets or sets the zodiac attribute.
        /// </summary>
        public string ZodiacAttribute
        {
            get => this.zodiacAttribute;
            set => this.SetProperty(ref this.zodiacAttribute, value);
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password
        {
            get => this.password;
            set => this.SetProperty(ref this.password, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether an operation is in progress.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets or sets the error message to display to the user.
        /// </summary>
        public string ErrorMessage
        {
            get => this.errorMessage;
            set => this.SetProperty(ref this.errorMessage, value);
        }

        /// <summary>
        /// Gets or sets the success message to display to the user.
        /// </summary>
        public string SuccessMessage
        {
            get => this.successMessage;
            set => this.SetProperty(ref this.successMessage, value);
        }

        /// <summary>
        /// Requests creation of a user profile with the current data.
        /// </summary>
        public void RequestProfileCreation()
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

            this.ProfileCreationRequested?.Invoke(this, user);
        }

        /// <summary>
        /// Requests navigation to the login page.
        /// </summary>
        public void RequestNavigateToLogin()
        {
            this.NavigateToLoginRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Validates the input fields.
        /// </summary>
        /// <returns>True if all inputs are valid; otherwise, false.</returns>
        public bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(this.Username) ||
                string.IsNullOrWhiteSpace(this.FirstName) ||
                string.IsNullOrWhiteSpace(this.LastName) ||
                string.IsNullOrWhiteSpace(this.Email) ||
                string.IsNullOrWhiteSpace(this.CNP) ||
                string.IsNullOrWhiteSpace(this.Password))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Resets the form to its initial state.
        /// </summary>
        public void ResetForm()
        {
            Image = string.Empty;
            Username = string.Empty;
            Description = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            Birthday = DateTimeOffset.Now;
            CNP = string.Empty;
            ZodiacSign = string.Empty;
            ZodiacAttribute = string.Empty;
            Password = string.Empty;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsLoading = false;
        }

        /// <summary>
        /// Sets the property value and raises the PropertyChanged event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">Reference to the backing field.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">The name of the property (automatically provided by the compiler).</param>
        /// <returns>True if the property value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
