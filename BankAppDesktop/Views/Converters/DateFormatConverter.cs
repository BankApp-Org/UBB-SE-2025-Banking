using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace BankAppDesktop.Views.Converters
{
    public partial class DateFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is DateTime dateTime && parameter is string format ? dateTime.ToString(format, CultureInfo.InvariantCulture) : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string dateString && parameter is string format)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                {
                    return dateTime;
                }
            }
            return value;
        }
    }
}
