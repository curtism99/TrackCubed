using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json; // For potential deserialization of the user object
using System.Threading.Tasks;
using TrackCubed.Maui.Services;
using TrackCubed.Maui.Views;

namespace TrackCubed.Maui.ViewModels
{
    public partial class LoginPageViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly HttpClient _httpClient;

        // *** CHANGE 1: Inject InitializationService and IServiceProvider ***
        // We need the InitializationService to run post-login tasks.
        // We need IServiceProvider to get a fresh instance of AppShell.
        private readonly InitializationService _initializationService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        // *** CHANGE 2: Update the constructor to accept the new services ***
        public LoginPageViewModel(AuthService authService, HttpClient httpClient, InitializationService initializationService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _httpClient = httpClient;
            _initializationService = initializationService;
            _serviceProvider = serviceProvider;
        }

        [RelayCommand]
        private async Task InteractiveLoginAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                AuthenticationResult authResult = await _authService.InteractiveLoginAsync();

                if (authResult == null || string.IsNullOrEmpty(authResult.AccessToken))
                {
                    await Shell.Current.DisplayAlert("Login Failed", "Could not sign you in. Please try again.", "OK");
                    return;
                }

                // OnboardUserWithApi will now return a boolean indicating success
                bool onboardSuccess = await OnboardUserWithApi(authResult.AccessToken);

                // *** CHANGE 3: The new navigation logic ***
                if (onboardSuccess)
                {
                    Debug.WriteLine("[LoginPageViewModel] Onboarding successful. Swapping to AppShell...");

                    // A. Run all the background data tasks AFTER successful onboarding.
                    await _initializationService.InitializeAfterLoginAsync();

                    // B. Replace the entire application's root page with a new AppShell.
                    // This is the clean, robust way to transition to the main app.
                    Application.Current.MainPage = _serviceProvider.GetRequiredService<AppShell>();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // *** CHANGE 4: Refactor OnboardUserWithApi to return a success/failure boolean ***
        private async Task<bool> OnboardUserWithApi(string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "api/user/onboard");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    // The API call worked. Return true.
                    return true;
                }
                else
                {
                    // The API call failed. Show an error and return false.
                    string errorContent = await response.Content.ReadAsStringAsync();
                    await Shell.Current.DisplayAlert("API Error", $"The server responded with an error: {response.ReasonPhrase}\n{errorContent}", "OK");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                // A network error occurred. Show an error and return false.
                await Shell.Current.DisplayAlert("Network Error", $"Could not connect to the server. Please check your connection and try again. Details: {ex.Message}", "OK");
                return false;
            }
        }
    }
}
