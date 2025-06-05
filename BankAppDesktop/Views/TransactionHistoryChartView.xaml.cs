using System.Windows.Controls;
using BankAppDesktop.ViewModels;

namespace BankAppDesktop.Views
{
    public partial class TransactionHistoryChartView : UserControl
    {
        public TransactionHistoryChartViewModel ViewModel { get; set; }

        public TransactionHistoryChartView()
        {
            this.InitializeComponent();
            ViewModel = new TransactionHistoryChartViewModel(/* TODO: injectează serviciul real */ null);
            this.DataContext = ViewModel;
        }
    }
} 