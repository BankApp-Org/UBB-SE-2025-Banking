namespace BankAppDesktop.Views.Converters
{
    using System;
    using Microsoft.UI.Xaml.Data;
    using Microsoft.UI.Xaml.Media;

    public partial class AmountColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int amount)
            {
                // Return Red if negative, Green if positive, Black is default color
                if (amount < 0)
                {
                    return new SolidColorBrush(Microsoft.UI.Colors.Red);
                }
                else
                {
                    return amount > 0 ? new SolidColorBrush(Microsoft.UI.Colors.Green) : new SolidColorBrush(Microsoft.UI.Colors.Black);
                }
            }

            return new SolidColorBrush(Microsoft.UI.Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
