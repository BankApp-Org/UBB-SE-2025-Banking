namespace BankAppDesktop.Views.Components
{
    using Common.Models;
    using Common.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using BankAppDesktop.Views.Pages;
    using System;

    public sealed partial class UserInfoComponent : Page
    {
        private readonly IActivityService activityService;
        private readonly IHistoryService historyService;
        public User? User;

        public UserInfoComponent(IActivityService activityService, IHistoryService historyService)
        {
            this.InitializeComponent();
            this.activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
            this.historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        }

        public void SetUserData(User userData)
        {
            this.User = userData;
            this.NameTextBlock.Text = $"{this.User.FirstName}  {this.User.LastName}";
            this.CNPTextBlock.Text = $"{this.User.CNP}";
            this.ScoreTextBlock.Text = $"Score: {this.User.CreditScore}";
        }

        private void OnAnalysisClick(object sender, RoutedEventArgs e)
        {
            if (this.User != null)
            {
                var analysisWindow = new AnalysisWindow(this.User, this.activityService, this.historyService);
                analysisWindow.Activate();
            }
        }

        private void OnTipHistoryClick(object seder, RoutedEventArgs e)
        {
            if (this.User != null)
            {
                TipHistoryWindow tipHistoryWindow = App.Host.Services.GetService<TipHistoryWindow>() ?? throw new InvalidOperationException("TipHistoryWindow not registered in DI container.");
                tipHistoryWindow.LoadUser(this.User);
                tipHistoryWindow.Activate();
            }
        }
    }
}
