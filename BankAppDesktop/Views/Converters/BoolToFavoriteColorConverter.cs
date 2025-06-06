namespace BankAppDesktop.Views.Converters
{
    using System;
    using Microsoft.UI;
    using Microsoft.UI.Xaml.Data;
    using Microsoft.UI.Xaml.Media;
    using Windows.UI;

    internal partial class BoolToFavoriteColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string lang)
        {
            if (value is bool boolValue)
            {
                if (parameter is string colorNames)
                {
                    try
                    {
                        var colors = colorNames.Split('|');
                        return boolValue ? new SolidColorBrush(ConvertStringToColor(colors[0])) : new SolidColorBrush(ConvertStringToColor(colors[1]));
                    }
                    catch
                    {
                        return new SolidColorBrush(Colors.Transparent);
                    }
                }
                else
                {
                    // Default colors if no parameter is provided
                    return boolValue ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                }
            }

            throw new InvalidOperationException("Value must be a boolean.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string lang)
        {
            throw new NotImplementedException("ConvertBack is not supported.");
        }

        private static Color ConvertStringToColor(string colorName)
        {
            // Use a predefined method to convert string to Color
            var colorProperty = typeof(Colors).GetProperty(colorName);
            return colorProperty != null && colorProperty.GetValue(null) is Color colorValue
                ? colorValue
                : throw new ArgumentException($"Invalid color name: {colorName}");
        }
    }
}
