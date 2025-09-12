using TrackCubed.Maui.Services;

namespace TrackCubed.Maui
{
    public partial class AppShell : Shell
    {
        // A static action that any part of the app can call to trigger a UI update.
        public static Action OnLoginStateChanged;
        private bool _isInitialCheckComplete = false;
        private readonly AuthService _authService;
        public AppShell(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;

            // Subscribe to the event so we can react to manual logins/logouts
            // We no longer call the check from the constructor.
            AppShell.OnLoginStateChanged += SetUiForLoginState;
        }

        // OnAppearing is a built-in MAUI lifecycle method that is called
        // when the Shell is about to be displayed on screen.
        // By this time, Shell.Current is guaranteed to be set.
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Only run this initial check the very first time the app appears.
            if (!_isInitialCheckComplete)
            {
                _isInitialCheckComplete = true;
                SetUiForLoginState();
            }
        }

        private async void SetUiForLoginState()
        {
            var accounts = await _authService.GetLoggedInAccountsAsync();

            // Use MainThread.BeginInvokeOnMainThread to ensure UI updates
            // and navigation happen on the correct thread, preventing potential crashes.
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (accounts.Any())
                {
                    // User is logged in
                    MainAppTabs.IsVisible = true;
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    // User is logged out
                    MainAppTabs.IsVisible = false;
                    await Shell.Current.GoToAsync("//LoginPage");
                }
            });
        }
    }
}
