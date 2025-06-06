namespace BankAppDesktop.Views.Converters
{
    using System;
    using Microsoft.UI;
    using Microsoft.UI.Xaml.Data;
    using Microsoft.UI.Xaml.Media;

    /// <summary>
    /// Converts a string value representing a change to a corresponding color.
    /// </summary>
    public partial class ChangeToColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts a string value to a SolidColorBrush based on its content.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type of the conversion.</param>
        /// <param name="parameter">Optional parameter for the conversion.</param>
        /// <param name="language">The language information for the conversion.</param>
        /// <returns>A SolidColorBrush representing the color.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is string change && !string.IsNullOrEmpty(change)
                ? change.StartsWith('+') ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red)
                : new SolidColorBrush(Colors.Black);
        }

        /// <summary>
        /// Not implemented. Converts back from a SolidColorBrush to a string value.
        /// </summary>
        /// <param name="value">The value to convert back.</param>
        /// <param name="targetType">The target type of the conversion.</param>
        /// <param name="parameter">Optional parameter for the conversion.</param>
        /// <param name="language">The language information for the conversion.</param>
        /// <returns>Throws a NotImplementedException.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
