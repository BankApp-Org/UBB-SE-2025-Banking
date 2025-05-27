namespace StockApp.Views.Converters
{
    using System;
    using Microsoft.UI.Xaml;

    public partial class BoolToVisibilityConverter : BaseConverter
    {
        /// <summary>
        /// Converts a boolean value to a Visibility value.
        /// Returns Visible if true, Collapsed if false. Supports inversion through a parameter.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="targetType">The target type of the binding (expected to be Visibility).</param>
        /// <param name="parameter">An optional parameter to indicate if the result should be inverted.</param>
        /// <param name="language">The culture language information.</param>
        /// <returns>Visibility.Visible if true; otherwise, Visibility.Collapsed. Inverted if specified.</returns>
        /// <exception cref="InvalidCastException">Thrown when the input value is not a boolean.</exception>
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not bool booleanValue)
            {
                throw new InvalidCastException("Expected a boolean value for BoolToVisibilityConverter.");
            }

            if (ShouldInvert(parameter))
            {
                booleanValue = !booleanValue;
            }

            return booleanValue
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a Visibility value back to a boolean.
        /// Returns true if Visible, false if Collapsed. Supports inversion through a parameter.
        /// </summary>
        /// <param name="value">The Visibility value to convert back.</param>
        /// <param name="targetType">The target type of the binding (expected to be bool).</param>
        /// <param name="parameter">An optional parameter to indicate if the result should be inverted.</param>
        /// <param name="language">The culture language information.</param>
        /// <returns>True if Visibility.Visible; otherwise, false. Inverted if specified.</returns>
        /// <exception cref="InvalidCastException">Thrown when the input value is not a Visibility enum.</exception>
        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is not Visibility visibilityValue
                ? throw new InvalidCastException("Expected a Visibility value for BoolToVisibilityConverter.")
                : (object)(ShouldInvert(parameter)
                ? visibilityValue != Visibility.Visible
                : visibilityValue == Visibility.Visible);
        }
    }
}
