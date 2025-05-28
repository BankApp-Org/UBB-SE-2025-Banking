namespace BankAppDesktop.ViewModels
{
    using Common.Models;
    using System.Collections.ObjectModel;

    /// <summary>
    /// ViewModel for users, containing only data properties.
    /// </summary>
    public class UserViewModel : ViewModelBase
    {
        private ObservableCollection<User> users = [];

        public ObservableCollection<User> Users
        {
            get => this.users;
            set => this.SetProperty(ref this.users, value);
        }

        public UserViewModel()
        {
        }
    }
}