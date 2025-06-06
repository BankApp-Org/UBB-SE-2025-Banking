using BankAppDesktop.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BankAppDesktop.Views.Pages
{
    public partial class NotificationsPage : Page
    {
        public NotificationsPage(NotificationsViewModel notificationsViewModel)
        {
            InitializeComponent();
            DataContext = notificationsViewModel;
        }
    }
}