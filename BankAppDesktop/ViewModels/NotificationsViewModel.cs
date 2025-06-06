using BankAppDesktop.Commands;
using Common.Models.Social;
using Common.Services;
using Common.Services.Social;
using Microsoft.UI.Xaml;
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
        private string _errorMessage;
        private bool _isLoading;
        private bool _hasError;

        public ObservableCollection<Notification> Notifications
        {
            get => _notifications;
            set => SetProperty(ref _notifications, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
                HasError = !string.IsNullOrEmpty(value);
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
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
            _errorMessage = string.Empty;

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
                IsLoading = true;
                ErrorMessage = string.Empty;

                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                {
                    ErrorMessage = "Please log in to view notifications.";
                    return;
                }

                var notifications = await _notificationService.GetNotificationsForUser(user.Id);

                Notifications.Clear();
                foreach (var notification in notifications)
                {
                    Notifications.Add(notification);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading notifications: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error loading notifications: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ClearNotification(int notificationId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                {
                    ErrorMessage = "Please log in to clear notifications.";
                    return;
                }

                await _notificationService.MarkNotificationAsRead(notificationId, user.Id);
                await LoadNotifications();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error clearing notification: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error clearing notification: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ClearAllNotifications()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                {
                    ErrorMessage = "Please log in to clear notifications.";
                    return;
                }

                await _notificationService.MarkAllNotificationsAsRead(user.Id);
                await LoadNotifications();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error clearing all notifications: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error clearing all notifications: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}