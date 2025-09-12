namespace TrackCubed.Maui
{
    public partial class App : Application
    {
        // Change the constructor to accept AppShell
        public App(AppShell appShell)
        {
            InitializeComponent();

            // Use the injected AppShell
            MainPage = appShell;
        }

    }
}