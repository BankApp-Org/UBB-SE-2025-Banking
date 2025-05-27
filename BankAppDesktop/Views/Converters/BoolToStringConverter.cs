namespace StockApp.Views.Converters
{
    using System;

    public partial class BoolToStringConverter : BaseConverter
    {
        /// <summary>
        /// Converts a boolean value to a string based on the provided parameter.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool booleanValue && parameter is string options)
            {
                var parts = options.Split('|');
                if (parts.Length == 2)
                {
                    return booleanValue ? parts[0] : parts[1];
                }
            }

            return value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Converts a string back to a boolean value. This method is not implemented in this converter.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException("ConvertBack is not implemented in BoolToStringConverter.");
        }
    }
}
