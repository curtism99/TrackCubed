using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackCubed.Maui.Services
{
    /// <summary>
    /// A service to manage the application's visual theme (Light/Dark/System).
    /// </summary>
    public class ThemeService
    {
        // A key to store the user's theme preference on the device.
        private const string ThemeKey = "AppTheme";

        /// <summary>
        /// Sets the application's current theme and saves the preference.
        /// </summary>
        /// <param name="theme">The name of the theme to apply ("Light", "Dark", or "System").</param>
        public void SetTheme(string theme)
        {
            AppTheme newTheme = theme switch
            {
                "Light" => AppTheme.Light,
                "Dark" => AppTheme.Dark,
                _ => AppTheme.Unspecified // "Unspecified" tells the app to follow the OS setting.
            };

            // Set the live theme for the currently running application.
            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = newTheme;
            }

            // Save the user's preference so it can be loaded next time the app starts.
            Preferences.Set(ThemeKey, theme);
        }

        /// <summary>
        /// Loads the user's saved theme preference from device storage.
        /// </summary>
        /// <returns>The saved theme name, or "System" if none is found.</returns>
        public string LoadTheme()
        {
            // Load the saved preference, defaulting to "System" if it has never been set.
            return Preferences.Get(ThemeKey, "System");
        }
    }
}
