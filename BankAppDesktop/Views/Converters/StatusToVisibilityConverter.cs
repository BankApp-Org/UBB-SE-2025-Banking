namespace StockApp.Views.Converters
{
    using System;
    using Microsoft.UI.Xaml;

    internal partial class StatusToVisibilityConverter : BaseConverter
    {
        /// <summary>
        /// Converts a status string to a Visibility value.
        /// Returns Collapsed if the status matches the expected status; otherwise, returns Visible.
        /// </summary>
        /// <param name="value">The current status string.</param>
        /// <param name="targetType">The target type of the binding (expected to be Visibility).</param>
        /// <param name="parameter">The expected status string used for comparison.</param>
        /// <param name="language">The culture language information.</param>
        /// <returns>Visibility.Collapsed if status matches expectedStatus; otherwise, Visibility.Visible.</returns>
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is not string status || parameter is not string expectedStatus
                ? Visibility.Collapsed
                : status == expectedStatus
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
            throw new NotImplementedException("ConvertBack is not implemented in StatusToVisibilityConverter.");
        }
    }
}
