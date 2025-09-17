using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using TrackCubed.Maui.Messages;
using TrackCubed.Maui.Services;
using TrackCubed.Maui.Views;

namespace TrackCubed.Maui
{
    public partial class AppShell : Shell, IRecipient<SignOutMessage>
    {
        public AppShell(AuthService authService, InitializationService initializationService)
        {
            InitializeComponent();

            WeakReferenceMessenger.Default.Register<SignOutMessage>(this);

            // --- Route Registrations ---

            // Register pages that can be navigated to directly.
            // This allows for calls like GoToAsync("LoginPage"), GoToAsync("MainPage"), etc.
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(AddCubedItemPage), typeof(AddCubedItemPage));

            // Register the TabBar itself as a route. 
            // This is necessary for the absolute navigation `//MainAppTabs` to work on startup.
            Routing.RegisterRoute("MainAppTabs", typeof(MainPage));
        }

        
        // This method is required by IRecipient and handles the incoming message
        public async void Receive(SignOutMessage message)
        {
            // A sign-out message was received. The value is true if sign-out was successful.
            if (message.Value)
            {
                Debug.WriteLine("[AppShell] SignOutMessage received. Navigating to LoginPage.");
                // Use a dispatcher to ensure navigation happens on the UI thread.
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await GoToAsync($"//{nameof(LoginPage)}");
                });
            }
        }
    }
}
