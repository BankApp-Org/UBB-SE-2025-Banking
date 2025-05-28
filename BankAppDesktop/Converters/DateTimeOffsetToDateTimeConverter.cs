using System;
using Microsoft.UI.Xaml.Data;

namespace BankAppDesktop.Converters
{
    public class DateTimeOffsetToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTimeOffset dateTimeOffset)
            {
                return dateTimeOffset.DateTime;
            }
            return value is DateTime dateTime ? dateTime : DateTime.Now;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dateTime)
            {
                return new DateTimeOffset(dateTime);
            }
            return value is DateTimeOffset dateTimeOffset ? dateTimeOffset : DateTimeOffset.Now;
        }
    }
}