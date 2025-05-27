namespace StockApp.Views.Converters
{
    using System;
    using Common.Exceptions;

    public partial class DateTimeToStringConverter : BaseConverter
    {
        /// <summary>
        /// Converts a DateTime value to its string representation using the specified format.
        /// </summary>
        /// <param name="value">The DateTime value to convert.</param>
        /// <param name="targetType">The target type of the binding (expected to be string).</param>
        /// <param name="parameter">An optional format string. Defaults to "MMMM dd, yyyy" if not provided.</param>
        /// <param name="language">The culture language information.</param>
        /// <returns>A formatted string representation of the DateTime value.</returns>
        /// <exception cref="InvalidCastException">Thrown when the input value is not a DateTime.</exception>
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            string format = parameter as string ?? "MMMM dd, yyyy";

            return value is not DateTime dateTimeValue
                ? throw new InvalidCastException("Expected DateTime value for DateTimeToStringConverter.")
                : (object)dateTimeValue.ToString(format);
        }

        /// <summary>
        /// Converts a string representation of a date back to a DateTime object.
        /// </summary>
        /// <param name="value">The string value to convert back.</param>
        /// <param name="targetType">The target type of the binding (expected to be DateTime).</param>
        /// <param name="parameter">An optional format string (not used in ConvertBack).</param>
        /// <param name="language">The culture language information.</param>
        /// <returns>A DateTime object parsed from the input string.</returns>
        /// <exception cref="InvalidCastException">Thrown when the input value is not a string.</exception>
        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is not string stringValue
                ? throw new InvalidCastException("Expected a string value for date conversion.")
                : (object)ParseOrDefault(stringValue);
        }

        private static DateTime ParseOrDefault(string input)
        {
            return DateTime.TryParse(input, out DateTime result)
                ? result
                : throw new ConverterException($"Invalid date format: '{input}'. Unable to parse.");
        }
    }
}
