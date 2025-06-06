using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAppDesktop.ViewModels
{
    using Common.Models;
    using Common.Services;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;

    public partial class AddFriendsViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<User> UsersList { get; set; }

        private FriendsListViewModel friendsListViewModel;

        private string searchQuery;

        public List<User> AllUsers { get; set; }

        public IUserService UserService { get; set; }

        public ICommand AddFriendCommand { get; set; }

        public string SearchQuery
        {
            get => this.searchQuery;
            set
            {
                if (this.searchQuery != value)
                {
                    this.searchQuery = value;
                    this.OnPropertyChanged(nameof(this.SearchQuery));
                    this.FilterUsers();
                }
            }
        }

        public AddFriendsViewModel(FriendsListViewModel friendsListViewModel, IUserService userService)
        {
            this.UserService = userService;
            this.friendsListViewModel = friendsListViewModel;
            this.AllUsers = [];
            this.LoadAllUsers();
            this.UsersList = [];
            // this.AddFriendCommand = new RelayCommand<object>(AddFriend);
            this.LoadUsers();
        }

        public async void LoadAllUsers()
        {
            this.AllUsers.Clear();
            var currentUser = await this.UserService.GetCurrentUserAsync();
            var allUsers = await (UserService.GetUsers());
            var allFriends = currentUser.Friends.ToList();
            this.AllUsers = allUsers.Where(u => !allFriends.Any(f => f.CNP == u.CNP)).ToList();
            this.UsersList.Clear();

            foreach (var friend in this.AllUsers)
            {
                this.UsersList.Add(friend);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void AddFriend(object user)
        {
            var friend = user as User;
            this.UserService.AddFriend(friend);
            this.friendsListViewModel.LoadFriends();
            this.LoadUsers();
        }

        private async void LoadUsers()
        {
            var currentUser = await this.UserService.GetCurrentUserAsync();
            var allUsers = await (UserService.GetUsers());
            var allFriends = currentUser.Friends.ToList();
            this.AllUsers = allUsers.Where(u => !allFriends.Any(f => f.CNP == u.CNP)).ToList();
            this.FilterUsers();
        }

        private void FilterUsers()
        {
            this.UsersList.Clear();

            foreach (var friend in this.AllUsers.Where(f =>
                         string.IsNullOrEmpty(this.SearchQuery) ||
                         f.FirstName.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase)))
            {
                this.UsersList.Add(friend);
            }
        }
    }
}
