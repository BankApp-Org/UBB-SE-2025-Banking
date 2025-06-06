using BankAppDesktop.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;

namespace BankAppDesktop.Views.Pages
{
    public partial class NotificationsPage : Page
    {
        private readonly NotificationsViewModel _viewModel;

        public NotificationsPage(NotificationsViewModel notificationsViewModel)
        {
            InitializeComponent();
            _viewModel = notificationsViewModel;
            DataContext = _viewModel;
            
            Debug.WriteLine($"NotificationsPage initialized with ViewModel: {_viewModel != null}");
            Debug.WriteLine($"NotificationListView DataContext: {NotificationListView.DataContext != null}");
            Debug.WriteLine($"Current notifications count: {_viewModel.Notifications?.Count ?? 0}");

            Loaded += NotificationsPage_Loaded;
        }

        private void NotificationsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("NotificationsPage Loaded event fired");
            Debug.WriteLine($"Page DataContext: {DataContext != null}");
            Debug.WriteLine($"ListView DataContext: {NotificationListView.DataContext != null}");
            Debug.WriteLine($"Notifications count: {_viewModel.Notifications?.Count ?? 0}");
            Debug.WriteLine($"ListView items count: {NotificationListView.Items.Count}");
        }

        private void ClearNotification_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int notificationId)
            {
                Debug.WriteLine($"Clearing notification with ID: {notificationId}");
                _viewModel.ClearNotificationCommand.Execute(notificationId);
            }
        }
    }
}