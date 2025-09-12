using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackCubed.Maui.Services;

namespace TrackCubed.Maui.ViewModels
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string _displayName;

        [ObservableProperty]
        private string _email;

        public SettingsPageViewModel(AuthService authService)
        {
            _authService = authService;
            Title = "Profile & Settings";
        }

        [ObservableProperty]
        private string _title;

        // Command to load the user's data when the page appears
        [RelayCommand]
        private void LoadUserInformation()
        {
            var (name, email) = _authService.GetCurrentUser();
            DisplayName = name;
            Email = email;
        }

        // The sign out logic is the same as on the MainPageViewModel
        [RelayCommand]
        private async Task SignOutAsync()
        {
            bool confirmed = await Shell.Current.DisplayAlert(
                "Sign Out",
                "Are you sure you want to sign out?",
                "Yes, Sign Out",
                "Cancel");

            if (confirmed)
            {
                await _authService.SignOutAsync();
                AppShell.OnLoginStateChanged?.Invoke();
            }
        }
    }
}
