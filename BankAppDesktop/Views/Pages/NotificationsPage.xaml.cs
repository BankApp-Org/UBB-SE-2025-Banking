using BankAppDesktop.ViewModels;
using System.Windows.Controls;

namespace BankAppDesktop.Views.Pages
{
    public partial class NotificationsPage : Page
    {
        public NotificationsPage()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService(typeof(NotificationsViewModel));
        }
    }
} 