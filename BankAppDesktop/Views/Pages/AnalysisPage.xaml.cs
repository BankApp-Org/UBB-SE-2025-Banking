using Common.Models;
using Common.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using BankAppDesktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Services.Bank;

namespace BankAppDesktop.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AnalysisPage : Page
    {
        private readonly IUserService _userService;
        private readonly IActivityService _activityService;
        private readonly IAuthenticationService _authenticationService;
        private ActivityViewModel _viewModel;

        public AnalysisPage(IUserService userService, IActivityService activityService, IAuthenticationService authenticationService, ActivityViewModel viewModel)
        {
            this.InitializeComponent();
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            this.DataContext = _viewModel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _viewModel.IsLoading = true;
                _viewModel.ErrorMessage = string.Empty;

                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    _viewModel.ErrorMessage = "Unable to identify user. Please log in again.";
                    return;
                }

                _viewModel.CurrentUser = currentUser;

                // Check if current user is admin
                _viewModel.IsAdmin = _authenticationService.IsUserAdmin();

                // If user is admin, load all users for dropdown
                if (_viewModel.IsAdmin)
                {
                    var users = await _userService.GetUsers();
                    _viewModel.UserList.Clear();
                    foreach (var user in users)
                    {
                        _viewModel.UserList.Add(new SelectListItem
                        {
                            Value = user.CNP,
                            Text = $"{user.UserName} - {user.FirstName} {user.LastName}",
                            Selected = user.CNP == currentUser.CNP
                        });
                    }

                    // Set the selected user CNP
                    _viewModel.SelectedUserCnp = currentUser.CNP;

                    // Get activities for current user initially
                    await LoadActivitiesForUser(currentUser.CNP);
                }
                else
                {
                    // Regular user - only get their own activities
                    _viewModel.SelectedUserCnp = currentUser.CNP;
                    await LoadActivitiesForUser(currentUser.CNP);
                }
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = $"Error loading user data: {ex.Message}";
            }
            finally
            {
                _viewModel.IsLoading = false;
            }
        }

        private async Task LoadActivitiesForUser(string userCnp)
        {
            try
            {
                var activities = await _activityService.GetActivityForUser(userCnp);
                _viewModel.Activities.Clear();
                foreach (var activity in activities)
                {
                    _viewModel.Activities.Add(activity);
                }
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = $"Error loading activities: {ex.Message}";
            }
        }

        private async void UserSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is SelectListItem selectedItem)
            {
                _viewModel.SelectedUserCnp = selectedItem.Value;
                await LoadActivitiesForUser(selectedItem.Value);
            }
        }
    }
}
