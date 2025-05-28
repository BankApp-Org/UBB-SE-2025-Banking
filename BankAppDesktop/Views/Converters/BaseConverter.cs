namespace BankAppDesktop.Views.Converters
{
    using System;
    using Microsoft.UI.Xaml.Data;

    public abstract class BaseConverter : IValueConverter
    {
        public abstract object Convert(object value, Type targetType, object parameter, string language);

        public abstract object ConvertBack(object value, Type targetType, object parameter, string language);

        protected static bool ShouldInvert(object parameter)
        {
            return parameter is string stringParameter
                && stringParameter.Equals("Inverse", StringComparison.OrdinalIgnoreCase);
        }
    }
}
