namespace BankAppDesktop.Views.Converters
{
    using System;
    using Microsoft.UI.Text;

    public partial class ReadStatusToFontWeightConverter : BaseConverter
    {
        /// <summary>
        /// Converts a read status boolean value to a FontWeight.
        /// Returns Normal if true (read), or SemiBold if false (unread).
        /// </summary>
        /// <param name="value">The boolean read status to convert.</param>
        /// <param name="targetType">The target type of the binding (expected to be FontWeight).</param>
        /// <param name="parameter">Optional parameter (not used).</param>
        /// <param name="language">The culture language information.</param>
        /// <returns>FontWeights.Normal if true; otherwise, FontWeights.SemiBold.</returns>
        /// <exception cref="InvalidCastException">Thrown when the input value is not a boolean.</exception>
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not bool statusIsReading)
            {
                throw new InvalidCastException("Expected a boolean value for ReadStatusToFontWeightConverter.");
            }

            // if read return normal, else return semi bold font for article title
            return statusIsReading
                ? FontWeights.Normal
                : FontWeights.SemiBold;
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
            throw new NotImplementedException("ConvertBack is not implemented in ReadStatusToFontWeightConverter.");
        }
    }
}
