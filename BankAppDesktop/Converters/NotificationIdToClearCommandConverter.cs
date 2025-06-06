using Microsoft.UI.Xaml.Data;
using System;
using System.Windows.Input;
using BankAppDesktop.ViewModels;
using Microsoft.UI.Xaml;

namespace BankAppDesktop.Converters
{
    public class NotificationIdToClearCommandConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int notificationId)
            {
                // Find the root frame
                var window = App.MainAppWindow;
                if (window?.MainAppFrame?.Content is FrameworkElement element)
                {
                    // Get the view model from the DataContext
                    if (element.DataContext is NotificationsViewModel viewModel)
                    {
                        // Return a command that will call ClearNotificationCommand with the notification ID
                        return viewModel.ClearNotificationCommand;
                    }
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 