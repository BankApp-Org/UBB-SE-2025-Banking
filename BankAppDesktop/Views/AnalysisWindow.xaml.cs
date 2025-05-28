namespace BankAppDesktop.Views
{
    using Common.Models;
    using Common.Services;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public sealed partial class AnalysisWindow : Window
    {
        private readonly User user;
        private readonly IActivityService activityService;
        private readonly IHistoryService historyService;
        private bool isLoading;

        public AnalysisWindow(User selectedUser, IActivityService activityService, IHistoryService historyService)
        {
            this.InitializeComponent();
            this.user = selectedUser;
            this.activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
            this.historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));

            this.LoadUserData();
            this.LoadUserActivitiesAsync();
            this.Init().ConfigureAwait(false);
        }

        public void LoadUserData()
        {
            this.IdTextBlock.Text = $"Id: {this.user.Id}";
            this.FirstNameTextBlock.Text = $"First name: {this.user.FirstName}";
            this.LastNameTextBlock.Text = $"Last name: {this.user.LastName}";
            this.CNPTextBlock.Text = $"CNP: {this.user.CNP}";
            this.EmailTextBlock.Text = $"Email: {this.user.Email}";
            this.PhoneNumberTextBlock.Text = $"Phone number: {this.user.PhoneNumber}";
        }

        private async Task Init()
        {
            this.LoadHistory(await this.historyService.GetHistoryMonthlyAsync(this.user.CNP));
        }

        public async void LoadUserActivitiesAsync()
        {
            if (isLoading)
            {
                return; // Prevent multiple simultaneous loads
            }

            try
            {
                isLoading = true;
                var activities = await this.activityService.GetActivityForUser(this.user.CNP);
                this.ActivityListView.ItemsSource = activities;
            }
            catch (Exception exception)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Error loading activities: {exception.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = App.MainAppWindow!.MainAppFrame.XamlRoot
                };
                await dialog.ShowAsync();
            }
            finally
            {
                isLoading = false;
            }
        }

        public void LoadHistory(List<CreditScoreHistory> history)
        {
            try
            {
                if (history.Count == 0)
                {
                    this.CreditScorePlotView.Model = new PlotModel { Title = "No credit history", };
                    return;
                }

                var plotModel = new PlotModel { Title = string.Empty, };

                var barSeries = new BarSeries
                {
                    Title = "Credit Score",
                    StrokeThickness = 1,
                };

                for (int i = 0; i < history.Count; i++)
                {
                    var record = history[i];
                    OxyColor barColor;

                    if (i == 0)
                    {
                        barColor = OxyColor.FromRgb(0, 255, 0);
                    }
                    else
                    {
                        var previousRecord = history[i - 1];
                        if (record.Score > previousRecord.Score)
                        {
                            barColor = OxyColor.FromRgb(0, 255, 0);
                        }
                        else if (record.Score == previousRecord.Score)
                        {
                            barColor = OxyColor.FromRgb(255, 255, 0);
                        }
                        else
                        {
                            barColor = OxyColor.FromRgb(255, 0, 0);
                        }
                    }

                    barSeries.Items.Add(new BarItem(record.Score, i) { Color = barColor });
                }

                plotModel.Series.Add(barSeries);

                var categoryAxis = new CategoryAxis
                {
                    Position = AxisPosition.Left,
                    Title = "Time",
                };

                for (int i = 0; i < history.Count; i++)
                {
                    categoryAxis.Labels.Add(history[i].Date.ToString("MM/dd/yyyy"));
                }

                plotModel.Axes.Add(categoryAxis);

                var valueAxis = new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    Title = "Score",
                    Minimum = 0,
                    Maximum = 800,
                };

                plotModel.Axes.Add(valueAxis);

                this.CreditScorePlotView.Model = plotModel;
            }
            catch (Exception exception)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Error loading history: {exception.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot,
                };
                _ = dialog.ShowAsync();
            }
        }

        private async void OnWeeklyClick(object sender, RoutedEventArgs e)
        {
            var history = await this.historyService.GetHistoryWeeklyAsync(this.user.CNP);
            this.LoadHistory(history);
        }

        private async void OnMonthlyClick(object sender, RoutedEventArgs e)
        {
            var history = await this.historyService.GetHistoryMonthlyAsync(this.user.CNP);
            this.LoadHistory(history);
        }

        private async void OnYearlyClick(object sender, RoutedEventArgs e)
        {
            var history = await this.historyService.GetHistoryYearlyAsync(this.user.CNP);
            this.LoadHistory(history);
        }
    }
}
