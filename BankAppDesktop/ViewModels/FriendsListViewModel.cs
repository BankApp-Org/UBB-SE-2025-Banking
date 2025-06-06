using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BankAppDesktop.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;
    using Microsoft.UI.Xaml;
    using System.Threading.Tasks;
    using Common.Models;
    using BankApi.Services;
    using BankApi.Services.Social;
    using Common.Services;
    using Common.Services.Social;

    public class FriendsListViewModel : INotifyPropertyChanged
    {
        public List<User> AllFriends { get; set; }

        public ObservableCollection<User> FriendsList { get; set; }

        public IUserService UserService { get; set; }

        public IChatService ChatService { get; set; }

        public IMessageService MessageService { get; set; }

        public ICommand RemoveFriend { get; }

        private Visibility noFriendsVisibility = Visibility.Collapsed;

        public Visibility NoFriendsVisibility
        {
            get
            {
                return this.noFriendsVisibility;
            }

            set
            {
                this.noFriendsVisibility = value;
                this.OnPropertyChanged(nameof(this.noFriendsVisibility));
            }
        }

        private string searchQuery;

        public string SearchQuery
        {
            get => this.searchQuery;
            set
            {
                if (this.searchQuery != value)
                {
                    this.searchQuery = value;
                    this.OnPropertyChanged(nameof(this.SearchQuery));
                    this.FilterFriends();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public FriendsListViewModel(IChatService chat, IUserService user, IMessageService message)
        {
            this.UserService = user;
            this.ChatService = chat;
            this.MessageService = message;
            this.AllFriends = [];
            this.FriendsList = [];
            LoadAllFriends();
            // this.RemoveFriend = new RelayCommand<object>(this.RemoveFriendFromList);
        }

        public async Task LoadAllFriends()
        {
            var currentUser = await this.UserService.GetCurrentUserAsync();
            this.AllFriends = currentUser.Friends.ToList();
            this.FriendsList.Clear();

            foreach (var friend in this.AllFriends)
            {
                this.FriendsList.Add(friend);
            }
        }

        public async void RemoveFriendFromList(object user)
        {
            var friend = user as User;

            var currentUser = await this.UserService.GetCurrentUserAsync();
            if (friend != null)
            {
                await this.UserService.RemoveFriend(friend);
            }

            LoadAllFriends();

            FriendsList = new ObservableCollection<User>(this.AllFriends);
        }

        public async void LoadFriends()
        {
            await LoadAllFriends();
            this.FilterFriends();
            FriendsList = new ObservableCollection<User>(this.AllFriends);
        }

        public void FilterFriends()
        {
            this.FriendsList.Clear();
            if (this.AllFriends != null)
            {
                foreach (var friend in this.AllFriends.Where(f =>
                             string.IsNullOrEmpty(this.SearchQuery) ||
                             f.FirstName.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                             f.PhoneNumber.ToString().Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase)))
                {
                    this.FriendsList.Add(friend);
                }
            }

            this.UpdatenoFriendsVisibility();
        }

        private void UpdatenoFriendsVisibility()
        {
            this.noFriendsVisibility = (this.FriendsList == null || this.FriendsList.Count == 0)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}

