using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackCubed.Maui.Messages;
using TrackCubed.Maui.Services;

namespace TrackCubed.Maui.ViewModels
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly ThemeService _themeService;
        private readonly CubedDataService _cubedDataService;
        private readonly WordBankService _wordBankService;

        [ObservableProperty]
        private string _displayName;

        [ObservableProperty]
        private string _email;

        // Property for "About" section
        [ObservableProperty] private string _appVersion;

        // Properties for Theme Selector
        public List<string> ThemeOptions { get; } = new List<string> { "System", "Light", "Dark" };

        [ObservableProperty]    
        private string _selectedTheme;


        public SettingsPageViewModel(AuthService authService, ThemeService themeService, CubedDataService cubedDataService, WordBankService wordBankService)
        {
            _authService = authService;
            _themeService = themeService;
            _cubedDataService = cubedDataService;
            _wordBankService = wordBankService;

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

        [RelayCommand]
        private async Task WipeAllDataAsync()
        {
            // 1. Generate a unique, hyphenated phrase from our new service.
            // This makes it impossible for the user to just muscle-memory their way through.
            string confirmationPhrase = _wordBankService.GetRandomPhrase();

            // 2. Show a special prompt dialog that includes a text entry field.
            string result = await Shell.Current.DisplayPromptAsync(
                "EXTREME DANGER: Wipe All Data?",
                $"This action is irreversible and will delete all of your Cubed Items and Tags. Please type '{confirmationPhrase}' to confirm.",
                "Confirm Wipe",
                "Cancel",
                placeholder: "Type confirmation phrase here");

            // 3. Check if the user's input matches the phrase.
            if (result != null && result.Equals(confirmationPhrase, StringComparison.Ordinal))
            {
                // 4. If it matches, call the data service.
                bool success = await _cubedDataService.WipeAllUserDataAsync();

                //bool success = true; // TEMPORARY: Remove this line when enabling the actual wipe.


                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "All your data has been wiped.", "OK");
                    // Notify the MainPage to refresh its (now empty) list.
                    WeakReferenceMessenger.Default.Send(new RefreshItemsMessage(true));
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to wipe data. Please try again.", "OK");
                }
            }
            else if (result != null) // User typed something, but it was wrong.
            {
                await Shell.Current.DisplayAlert("Cancelled", "The confirmation phrase did not match. No data was deleted.", "OK");
            }
            // If result is null, the user hit "Cancel". Do nothing.
        }

        [RelayCommand]
        private async Task CleanUpOrphanedTagsAsync()
        {
            bool confirmed = await Shell.Current.DisplayAlert(
                "Clean Up Tags",
                "This will permanently delete any tags that are not currently attached to any of your Cubed Items. This can't be undone. Continue?",
                "Yes, Clean Up",
                "Cancel");

            if (!confirmed) return;

            // Call the service and get the count of deleted tags
            int deletedCount = await _cubedDataService.CleanUpOrphanedTagsAsync();

            if (deletedCount > 0)
            {
                await Shell.Current.DisplayAlert("Success", $"Successfully deleted {deletedCount} orphaned tag(s).", "OK");
            }
            else if (deletedCount == 0)
            {
                await Shell.Current.DisplayAlert("All Clean!", "No orphaned tags were found.", "OK");
            }
            else // deletedCount is -1
            {
                await Shell.Current.DisplayAlert("Error", "Failed to clean up tags. Please try again.", "OK");
            }
        }

        [RelayCommand]
        private async Task CleanUpOrphanedItemTypesAsync()
        {
            if (!await Shell.Current.DisplayAlert(
                "Clean Up Item Types",
                "This will permanently delete any custom item types that are not currently used by any of your Cubed Items. Continue?",
                "Yes, Clean Up",
                "Cancel"))
            {
                return;
            }

            int deletedCount = await _cubedDataService.CleanUpOrphanedItemTypesAsync();

            if (deletedCount > 0)
            {
                await Shell.Current.DisplayAlert("Success", $"Successfully deleted {deletedCount} unused custom item type(s).", "OK");
            }
            else if (deletedCount == 0)
            {
                await Shell.Current.DisplayAlert("All Clean!", "No unused custom item types were found.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to clean up item types. Please try again.", "OK");
            }
        }

    }
}
