namespace StockApp.Views.Converters
{
    using System;
    using Microsoft.UI.Xaml.Data;

    /// <summary>
    /// Converts a boolean value indicating a favorite status to a star symbol.
    /// </summary>
    public class FavoriteToStarConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a star symbol.
        /// </summary>
        /// <param name="value">The value to convert, expected to be a boolean.</param>
        /// <param name="targetType">The target type of the conversion (not used).</param>
        /// <param name="parameter">An optional parameter (not used).</param>
        /// <param name="language">The culture information (not used).</param>
        /// <returns>A filled star (★) if true, otherwise an outline star (☆).</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isFavorite)
            {
                return isFavorite ? "★" : "☆";
            }

            return "☆"; // Default to outline star
        }

        /// <summary>
        /// ConvertBack is not implemented for this converter.
        /// </summary>
        /// <param name="value">The value to convert back (not used).</param>
        /// <param name="targetType">The target type of the conversion (not used).</param>
        /// <param name="parameter">An optional parameter (not used).</param>
        /// <param name="language">The culture information (not used).</param>
        /// <returns>Throws a NotImplementedException.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
