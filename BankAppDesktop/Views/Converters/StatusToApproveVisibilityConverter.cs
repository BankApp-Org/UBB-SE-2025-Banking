namespace BankAppDesktop.Views.Converters
{
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Data;
    using Common.Models;

    /// <summary>
    /// Converter that returns Visible for articles that can be approved (Pending status), otherwise Collapsed.
    /// </summary>
    public partial class StatusToApproveVisibilityConverter : BaseConverter
    {
        public override object Convert(object value, System.Type targetType, object parameter, string language)
        {
            if (value is Status status)
            {
                return status == Status.Pending ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public override object ConvertBack(object value, System.Type targetType, object parameter, string language)
        {
            throw new System.NotImplementedException();
        }
    }
}
