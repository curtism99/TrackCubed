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
#if DEBUG && WINDOWS
                    // Local development API.
                    BaseAddress = new Uri("http://localhost:5231")
#else
                    // Production API.
                    BaseAddress = new Uri("https://trackcubedapi20250911232429-b5hvbgdfd8hmbehe.centralus-01.azurewebsites.net")
#endif
                };
            });


            // Register Services
#if DEBUG && WINDOWS
            builder.Services.AddSingleton<IAuthService, DevelopmentAuthService>();
#else
            builder.Services.AddSingleton<IAuthService, AuthService>();
#endif
            builder.Services.AddSingleton<CubedDataService>();
            builder.Services.AddSingleton<ThemeService>();
            builder.Services.AddSingleton<WordBankService>();
            builder.Services.AddSingleton<InitializationService>();

            // Register ViewModels
            builder.Services.AddTransient<LoginPageViewModel>();
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<AddCubedItemViewModel>();
            builder.Services.AddTransient<SettingsPageViewModel>();

            // Register Pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<AddCubedItemPage>();
            builder.Services.AddTransient<SettingsPage>();

            builder.Services.AddSingleton<AppShell>();


            return builder.Build();
        }
    }
}
