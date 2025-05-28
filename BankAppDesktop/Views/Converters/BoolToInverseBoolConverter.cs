using System;

namespace BankAppDesktop.Views.Converters
{
    internal partial class BoolToInverseBoolConverter : BaseConverter
    {
        /// <summary>
        /// Converts a boolean value to its inverse (true to false, false to true).
        /// </summary>
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not bool booleanValue)
            {
                throw new InvalidCastException("Expected a boolean value for BoolToInverseBoolConverter.");
            }
            // Optionally support inversion via parameter, but for this converter, always invert
            return !booleanValue;
        }

        /// <summary>
        /// Converts back the value to its inverse boolean.
        /// </summary>
        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is not bool booleanValue)
            {
                throw new InvalidCastException("Expected a boolean value for BoolToInverseBoolConverter.");
            }
            return !booleanValue;
        }
    }
}
