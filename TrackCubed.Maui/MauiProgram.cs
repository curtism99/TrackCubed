using Microsoft.Extensions.Logging;
using TrackCubed.Maui.Views;
using TrackCubed.Maui.Services;
using TrackCubed.Maui.ViewModels;

namespace TrackCubed.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            // Register a singleton HttpClient for the entire application.
            // This instance is configured once with the production API's base address.
            builder.Services.AddSingleton<HttpClient>(serviceProvider =>
            {
                return new HttpClient
                {
                    // Set the single, production base address for your deployed API.
                    BaseAddress = new Uri("https://trackcubedapi20250911232429-b5hvbgdfd8hmbehe.centralus-01.azurewebsites.net")
                };
            });

            builder.Services.AddSingleton<AppShell>();

            // Register Services
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<CubedDataService>();

            // Register ViewModels
            builder.Services.AddTransient<LoginPageViewModel>();
            builder.Services.AddTransient<MainPageViewModel>();

            // Register Pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<MainPage>();

            return builder.Build();
        }
    }
}
