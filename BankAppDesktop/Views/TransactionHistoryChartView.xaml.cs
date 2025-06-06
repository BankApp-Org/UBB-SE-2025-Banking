using BankAppDesktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace BankAppDesktop.Views
{
    public partial class TransactionHistoryChartView : UserControl
    {
        public TransactionHistoryChartViewModel ViewModel { get; set; }

        public TransactionHistoryChartView()
        {
            this.InitializeComponent();
            ViewModel = App.Host.Services.GetRequiredService<TransactionHistoryChartViewModel>();
            this.DataContext = ViewModel;
        }
    }
}