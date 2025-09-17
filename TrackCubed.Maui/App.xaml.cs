using TrackCubed.Maui.Views;

namespace TrackCubed.Maui
{
    public partial class App : Application
    {
        // Change the constructor to accept AppShell
        public App(IServiceProvider services)
        {
            InitializeComponent();

            // Use the injected AppShell
            // CHANGE THIS:
            // MainPage = new AppShell(); 

            // TO THIS:
            // The app now starts directly on the LoginPage. The AppShell is not created yet.
            MainPage = services.GetRequiredService<LoginPage>();
        }

    }
}