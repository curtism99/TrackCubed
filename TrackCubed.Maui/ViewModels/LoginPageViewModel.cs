using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net.Http.Headers;
using System.Text.Json; // For potential deserialization of the user object
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TrackCubed.Maui.Services;

namespace TrackCubed.Maui.ViewModels
{
    public partial class LoginPageViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly HttpClient _httpClient;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        public LoginPageViewModel(AuthService authService, HttpClient httpClient)
        {
            _authService = authService;
            _httpClient = httpClient;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                string accessToken = await _authService.LoginAsync();

                if (string.IsNullOrEmpty(accessToken))
                {
                    // User might have cancelled the login
                    await Shell.Current.DisplayAlert("Login Failed", "Could not acquire access token. Please try again.", "OK");
                    return;
                }

                // Call the onboarding API endpoint
                await OnboardUserWithApi(accessToken);
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

        private async Task OnboardUserWithApi(string token)
        {
            try
            {
                // Create the request and add the token to the header for authorization
                var request = new HttpRequestMessage(HttpMethod.Post, "api/user/onboard");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    // User is now logged in and registered in our backend!
                    // Navigate to the main app page. This assumes you have a route named "MainPage"
                    // defined in your AppShell.xaml.
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    // The API returned an error (e.g., 401 Unauthorized, 500 Server Error)
                    string errorContent = await response.Content.ReadAsStringAsync();
                    await Shell.Current.DisplayAlert("API Error", $"The server responded with an error: {response.ReasonPhrase}\n{errorContent}", "OK");
                }
            }
            catch (HttpRequestException ex)
            {
                // This catches network-related errors (e.g., API is offline)
                await Shell.Current.DisplayAlert("Network Error", $"Could not connect to the server. Please check your connection and try again. Details: {ex.Message}", "OK");
            }
        }
    }
}
