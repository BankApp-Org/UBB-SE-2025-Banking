namespace StockApp.ViewModels
{
    using Common.Models;
    using Common.Services;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public partial class UserViewModel(IUserService userServices) : INotifyPropertyChanged
    {
        private readonly IUserService userService = userServices ?? throw new ArgumentNullException(nameof(userServices));

        public ObservableCollection<User> Users { get; set; } = [];

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task LoadUsers()
        {
            try
            {
                var users = await this.userService.GetUsers();
                foreach (var user in users)
                {
                    this.Users.Add(user);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error: {exception.Message}");
            }
        }
    }
}