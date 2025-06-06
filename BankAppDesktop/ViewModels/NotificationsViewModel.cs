using BankAppDesktop.Commands;
using Common.Models.Social;
using Common.Services;
using Common.Services.Social;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;

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
            get
            {
                Debug.WriteLine($"Notifications getter called, count: {_notifications?.Count ?? 0}");
                return _notifications;
            }
            set
            {
                Debug.WriteLine($"Notifications setter called with value count: {value?.Count ?? 0}");
                if (_notifications != value)
                {
                    _notifications = value;
                    OnPropertyChanged(nameof(Notifications));
                    Debug.WriteLine("Notifications property changed notification sent");
                }
            }
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
            Debug.WriteLine("NotificationsViewModel constructor called");
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _notifications = new ObservableCollection<Notification>();
            _errorMessage = string.Empty;

            ClearNotificationCommand = new RelayCommand(async o =>
            {
                if (o is int notificationId)
                {
                    await ClearNotification(notificationId);
                }
            }, o => o is int);
            ClearAllNotificationsCommand = new RelayCommand(async o => await ClearAllNotifications());
            RefreshCommand = new RelayCommand(async o => await LoadNotifications());

            Debug.WriteLine("Starting initial notifications load");
            _ = LoadNotifications();
        }

        private async Task LoadNotifications()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                Debug.WriteLine("Starting to load notifications...");

                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                {
                    ErrorMessage = "Please log in to view notifications.";
                    Debug.WriteLine("No user found - not logged in");
                    return;
                }
                Debug.WriteLine($"Current user ID: {user.Id}");

                var notifications = await _notificationService.GetNotificationsForUser(user.Id);
                Debug.WriteLine($"Retrieved {notifications?.Count ?? 0} notifications");

                if (notifications == null)
                {
                    Debug.WriteLine("Notifications list is null!");
                    return;
                }

                // Ensure UI updates happen on the UI thread
                await App.MainAppWindow.MainAppFrame.DispatcherQueue.EnqueueAsync(() =>
                {
                    Debug.WriteLine("Creating new notifications collection");
                    var newNotifications = new ObservableCollection<Notification>();
                    
                    Debug.WriteLine("Current collection state:");
                    Debug.WriteLine($"- Old collection count: {_notifications?.Count ?? 0}");
                    Debug.WriteLine($"- New notifications to add: {notifications.Count}");
                    
                    foreach (var notification in notifications)
                    {
                        Debug.WriteLine($"Adding notification: ID={notification.NotificationID}, Content={notification.Content}, UserId={notification.UserId}");
                        newNotifications.Add(notification);
                    }
                    
                    Debug.WriteLine($"Setting Notifications property with {newNotifications.Count} items");
                    Notifications = newNotifications;
                    Debug.WriteLine($"Final notifications count in collection: {Notifications.Count}");
                    Debug.WriteLine($"Collection reference changed: {!ReferenceEquals(_notifications, newNotifications)}");
                    
                    // Force a UI refresh
                    OnPropertyChanged(nameof(Notifications));
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading notifications: {ex.Message}";
                Debug.WriteLine($"Error loading notifications: {ex}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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
                Debug.WriteLine($"Error clearing notification: {ex}");
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
                Debug.WriteLine($"Error clearing all notifications: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public static class DispatcherQueueExtensions
    {
        public static Task EnqueueAsync(this Microsoft.UI.Dispatching.DispatcherQueue dispatcher, Action action)
        {
            var tcs = new TaskCompletionSource();
            if (!dispatcher.TryEnqueue(() =>
            {
                try
                {
                    action();
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }))
            {
                tcs.SetException(new Exception("Failed to enqueue task on dispatcher"));
            }
            return tcs.Task;
        }
    }
}