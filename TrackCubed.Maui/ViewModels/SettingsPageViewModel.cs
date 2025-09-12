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
        private readonly ThemeService _themeService;

        [ObservableProperty]
        private string _displayName;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _title;

        // Property for "About" section
        [ObservableProperty] private string _appVersion;

        // Properties for Theme Selector
        public List<string> ThemeOptions { get; } = new List<string> { "System", "Light", "Dark" };

        [ObservableProperty]    
        private string _selectedTheme;


        public SettingsPageViewModel(AuthService authService, ThemeService themeService)
        {
            _authService = authService;
            _themeService = themeService;
            Title = "Settings";
            AppVersion = AppInfo.Current.VersionString;

            // Load the saved theme when the ViewModel is created
            SelectedTheme = _themeService.LoadTheme();
        }

 
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

        // This method automatically runs when the Picker selection changes
        partial void OnSelectedThemeChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _themeService.SetTheme(value);
            }
        }

        [RelayCommand]
        private async Task OpenPrivacyPolicyAsync()
        {
            try
            {
                await Browser.Default.OpenAsync("https://www.trackcubed.com/privacy", BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception)
            {
                await Shell.Current.DisplayAlert("Error", "Could not open browser.", "OK");
            }
        }

    }
}
