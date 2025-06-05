namespace BankAppDesktop.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;
    using Microsoft.UI.Xaml.Controls;
    using BankAppDesktop.Commands;
    using Common.Models;
    using Common.Services;
    using Common.Services.Social;
    using Common.Models.Social;

    public class AddNewMemberViewModel : INotifyPropertyChanged
    {
        private List<User> allUnaddedFriends;
        private IUserService userService;
        private IChatService chatService;
        private Page lastChat;
        private string searchQuery;
        private int chatID;
        private ChatMessagesViewModel chatMessagesViewModel;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<User> UnaddedFriends { get; set; }

        public ObservableCollection<User> CurrentChatMembers { get; set; }

        public ObservableCollection<User> NewlyAddedFriends { get; set; }

        public string ChatName { get; set; }

        public ICommand AddToSelectedCommand { get; set; }

        public ICommand RemoveFromSelectedCommand { get; set; }

        public ICommand AddUsersToChatCommand { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AddNewMemberViewModel(ChatMessagesViewModel chatMessagesViewModel, Page lastChat, int chatID, IChatService chat, IUserService user)
        {
            this.chatMessagesViewModel = chatMessagesViewModel;
            this.chatID = chatID;
            this.lastChat = lastChat;
            this.userService = user;
            this.chatService = chat;
            this.LoadChatName();

            this.UnaddedFriends = new ObservableCollection<User>();
            this.CurrentChatMembers = new ObservableCollection<User>();
            this.NewlyAddedFriends = new ObservableCollection<User>();

            // this.AddToSelectedCommand = new RelayCommand<User>(this.AddToSelected);
            // this.RemoveFromSelectedCommand = new RelayCommand<User>(this.RemoveFromSelected);
            // this.AddUsersToChatCommand = new RelayCommand(this.AddUsersToChat);
            this.UpdateObservableLists();
        }

        public async void LoadChatName()
        {
            var chat = await this.chatService.GetChatById(this.chatID);
            this.ChatName = chat.ChatName;
        }

        public async void AddUsersToChat()
        {
            var chat = await this.chatService.GetChatById(this.chatID);
            foreach (User user in this.NewlyAddedFriends)
            {
                await this.chatService.AddUserToChat(chatID, user);
            }

            this.NewlyAddedFriends.Clear();
            this.UpdateObservableLists();

            this.chatMessagesViewModel.CurrentChatParticipants = chat.Users;
        }

        public void AddToSelected(User user)
        {
            this.NewlyAddedFriends.Add(user);
            this.UnaddedFriends.Remove(user);
        }

        public void RemoveFromSelected(User user)
        {
            this.NewlyAddedFriends.Remove(user);
            this.UnaddedFriends.Add(user);
        }

        public string SearchQuery
        {
            get
            {
                return this.searchQuery;
            }

            set
            {
                if (this.searchQuery != value)
                {
                    this.searchQuery = value;
                    this.OnPropertyChanged(nameof(this.SearchQuery));
                    this.UpdateFilteredFriends();
                }
            }
        }

        public async void LoadAllUnaddedFriendsList()
        {
            var currentUser = await this.userService.GetCurrentUserAsync();
            var allFriends = currentUser.Friends;
            var chat = await this.chatService.GetChatById(this.chatID);
            var currentChatParticipants = chat.Users;
            this.allUnaddedFriends = allFriends
                .Where(friend => friend != null && !currentChatParticipants.Any(participant => participant.Id == friend.Id))
                .ToList() ?? new List<User>();
        }

        public async void UpdateObservableLists()
        {
            var currentUser = await this.userService.GetCurrentUserAsync();
            var allFriends = currentUser.Friends;
            var chat = await this.chatService.GetChatById(this.chatID);
            var currentChatParticipants = chat.Users;
            this.allUnaddedFriends = allFriends
                .Where(friend => friend != null && !currentChatParticipants.Any(participant => participant.Id == friend.Id))
                .ToList() ?? new List<User>();

            this.CurrentChatMembers.Clear();
            var chatParticipantList = chat.Users;
            foreach (var participant in chatParticipantList)
            {
                this.CurrentChatMembers.Add(participant);
            }

            this.UpdateFilteredFriends();
            foreach (var friend in this.allUnaddedFriends)
            {
                this.UnaddedFriends.Add(friend);
            }
        }

        public void UpdateFilteredFriends()
        {
            this.UnaddedFriends.Clear();
            foreach (var friend in this.allUnaddedFriends.Where(f =>
                 string.IsNullOrEmpty(this.SearchQuery) ||
                 f.UserName?.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) == true ||
                 f.PhoneNumber?.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) == true))
            {
                this.UnaddedFriends.Add(friend);
            }
        }
    }
}
