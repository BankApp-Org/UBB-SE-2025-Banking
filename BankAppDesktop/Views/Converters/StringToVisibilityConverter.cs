namespace StockApp.Views.Converters
{
    using System;
    using Microsoft.UI.Xaml;

    public partial class StringToVisibilityConverter : BaseConverter
    {
        /// <summary>
        /// Converts a string value to a Visibility value.
        /// Returns Collapsed if the string is null or empty; otherwise, returns Visible.
        /// If the parameter is "Inverse", the logic is inverted.
        /// </summary>
        /// <param name="value">The string value to evaluate.</param>
        /// <param name="targetType">The target type of the binding (expected to be Visibility).</param>
        /// <param name="parameter">Optional parameter to invert logic if set to "Inverse".</param>
        /// <param name="language">The culture language information.</param>
        /// <returns>Visibility.Visible if string is not empty (or empty if inverted); otherwise, Visibility.Collapsed.</returns>
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not string stringValue)
            {
                return Visibility.Collapsed;
            }

            bool isEmpty = string.IsNullOrEmpty(stringValue);

            // if parameter is "Inverse", invert the logic
            if (ShouldInvert(parameter))
            {
                isEmpty = !isEmpty;
            }

            return isEmpty
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        /// <summary>
        /// ConvertBack is not implemented for this converter.
        /// </summary>
        /// <param name="value">The value to convert back (not used).</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">Optional parameter (not used).</param>
        /// <param name="language">The culture language information.</param>
        /// <returns>Throws NotImplementedException.</returns>
        /// <exception cref="NotImplementedException">Always thrown as ConvertBack is not implemented.</exception>
        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException("ConvertBack is not implemented in StringToVisibilityConverter.");
        }
    }
}
