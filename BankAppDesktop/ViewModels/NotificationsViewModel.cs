using BankAppDesktop.Commands;
using Common.Models.Social;
using Common.Services;
using Common.Services.Social;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BankAppDesktop.ViewModels
{
    public partial class NotificationsViewModel : ViewModelBase
    {
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private ObservableCollection<Notification> _notifications;

        public ObservableCollection<Notification> Notifications
        {
            get => _notifications;
            set => SetProperty(ref _notifications, value);
        }

        public ICommand ClearNotificationCommand { get; }
        public ICommand ClearAllNotificationsCommand { get; }
        public ICommand RefreshCommand { get; }

        public NotificationsViewModel(
            INotificationService notificationService,
            IUserService userService)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _notifications = [];

            Notifications = [];

            ClearNotificationCommand = new RelayCommand(async o =>
            {
                if (o is int notificationId)
                {
                    await ClearNotification(notificationId);
                }
            }, o => o is int);
            ClearAllNotificationsCommand = new RelayCommand(async o => await ClearAllNotifications());
            RefreshCommand = new RelayCommand(async o => await LoadNotifications());

            _ = LoadNotifications();
        }

        private async Task LoadNotifications()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                var notifications = await _notificationService.GetNotificationsForUser(user.Id);

                Notifications.Clear();
                foreach (var notification in notifications)
                {
                    Notifications.Add(notification);
                }
            }
            catch (Exception ex)
            {
                // Handle error appropriately
                System.Diagnostics.Debug.WriteLine($"Error loading notifications: {ex}");
            }
        }

        private async Task ClearNotification(int notificationId)
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                await _notificationService.MarkNotificationAsRead(notificationId, user.Id);
                await LoadNotifications();
            }
            catch (Exception ex)
            {
                // Handle error appropriately
                System.Diagnostics.Debug.WriteLine($"Error clearing notification: {ex}");
            }
        }

        private async Task ClearAllNotifications()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                await _notificationService.MarkAllNotificationsAsRead(user.Id);
                await LoadNotifications();
            }
            catch (Exception ex)
            {
                // Handle error appropriately
                System.Diagnostics.Debug.WriteLine($"Error clearing all notifications: {ex}");
            }
        }
    }
}