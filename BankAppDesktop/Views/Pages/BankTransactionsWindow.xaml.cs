using BankAppDesktop.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace BankAppDesktop.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BankTransactionsWindow : Window
    {
        private readonly BankTransactionsViewModel viewModel;
        private AppWindow appWindow;
        public BankTransactionsWindow()
        {
            this.InitializeComponent();
            viewModel = new BankTransactionsViewModel();
            MainGrid.DataContext = viewModel;
            viewModel.CloseAction = CloseWindow;

            // WindowManager.RegisterWindow(this);
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            var windowHandle = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
            appWindow = AppWindow.GetFromWindowId(windowId);

            if (appWindow != null)
            {
                appWindow.Resize(new Windows.Graphics.SizeInt32(1000, 800));
            }
        }

        private void CloseWindow()
        {
            this.Close();
        }
    }
}
