using System.Diagnostics;
using TrackCubed.Maui.Services;
using TrackCubed.Maui.ViewModels;

namespace TrackCubed.Maui.Views;

public partial class LoginPage : ContentPage
{
    // A static flag to ensure the silent sign-in check only runs once per app launch.
    private static bool _isInitialCheckComplete = false;

    private readonly IServiceProvider _services;

    public LoginPage(IServiceProvider services)
	{
		InitializeComponent();
        _services = services;
        BindingContext = _services.GetRequiredService<LoginPageViewModel>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_isInitialCheckComplete) return;
        _isInitialCheckComplete = true;

        // Use BeginInvokeOnMainThread to ensure this runs after the page is fully appeared.
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var authService = _services.GetRequiredService<AuthService>();
            var initializationService = _services.GetRequiredService<InitializationService>();

            try
            {
                var token = await authService.SilentSignInAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    await initializationService.InitializeAfterLoginAsync();

                    // *** THE FINAL FIX ***
                    // Add a very small, non-blocking delay.
                    // This allows the UI thread to finish its current work
                    // before starting the heavy task of building the next page.
                    await Task.Delay(50); // 50 milliseconds is often enough.

                    Application.Current.MainPage = _services.GetRequiredService<AppShell>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoginPage] Critical error during silent sign-in: {ex.Message}");
            }
        });
    }
}