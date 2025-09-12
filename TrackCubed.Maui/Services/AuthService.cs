using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackCubed.Maui.Configs;

namespace TrackCubed.Maui.Services
{
       public class AuthService
    {
        private IPublicClientApplication _pca;
        private AuthenticationResult _authResult;

#if WINDOWS
        // This helper is only used on the Windows platform.
        private MsalCacheHelper _cacheHelper;
#endif

        public AuthService()
        {
            // This setup MUST be done asynchronously. We call a helper to do this.
            // This is one of the few acceptable use cases for "async void" in a constructor.
            InitializeMsalClientAsync();
        }


        private async void InitializeMsalClientAsync()
        {
#if WINDOWS
            // --- WINDOWS-SPECIFIC DESKTOP CACHING LOGIC ---
            var storageProperties =
                new StorageCreationPropertiesBuilder("trackcubed.msalcache.bin", FileSystem.AppDataDirectory)
                .WithMacKeyChain(serviceName: "trackcubed_msal_service", accountName: "trackcubed_msal_account")
                .WithLinuxKeyring(
                    schemaName: "com.trackcubed.tokencache",
                    collection: "default",
                    secretLabel: "MSAL token cache for TrackCubed",
                    attribute1: new KeyValuePair<string, string>("AppName", "TrackCubed"),
                    attribute2: new KeyValuePair<string, string>("Version", "1.0.0.0"))
                .Build();

            // 2. Create the MsalCacheHelper. This object is responsible for saving the cache to a file.
            _cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);

            // 3. Build the PublicClientApplication, now with caching enabled.
            _pca = PublicClientApplicationBuilder.Create(EntraIdConstants.ClientId)
                        .WithTenantId(EntraIdConstants.TenantId)
                        .WithDefaultRedirectUri()
                        .Build();

            // 4. Register the cache helper with the MSAL client. This is the crucial step.
            _cacheHelper.RegisterCache(_pca.UserTokenCache);

#else
            // --- ANDROID / iOS / MACCATALYST LOGIC ---
            // On mobile platforms, we use the default builder without the file cache helper.
            // MSAL handles the in-memory cache automatically.
            _pca = PublicClientApplicationBuilder.Create(EntraIdConstants.ClientId)
                        .WithTenantId(EntraIdConstants.TenantId)
                        .WithDefaultRedirectUri()
                        .Build();
            await Task.CompletedTask; // To make the method async

#endif
        }



        private async Task WaitForInitialization()
        {
            while (_pca == null)
            {
                await Task.Delay(100).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Attempts to sign in the user silently using the cached token.
        /// </summary>
        /// <returns>An access token if successful, otherwise null.</returns>
        public async Task<string> SilentSignInAsync()
        {
            await WaitForInitialization();
            try
            {
                var accounts = await _pca.GetAccountsAsync();
                if (accounts.Any())
                {
                    _authResult = await _pca.AcquireTokenSilent(EntraIdConstants.Scopes, accounts.FirstOrDefault())
                                            .ExecuteAsync().ConfigureAwait(false);
                    
                    return _authResult?.AccessToken;
                }
            }
            catch (MsalUiRequiredException)
            {
                // This is expected if the token has expired or the user needs to re-consent.
                // It's a signal to fall back to interactive login.
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during silent sign-in: {ex.Message}");
                return null;
            }

            return null;
        }

        /// <summary>
        /// Performs the interactive sign-in flow, showing the browser UI.
        /// </summary>
        /// <returns>An access token if successful, otherwise null.</returns>
        public async Task<string> InteractiveLoginAsync()
        {
            await WaitForInitialization();
            try
            {
                _authResult = await _pca.AcquireTokenInteractive(EntraIdConstants.Scopes)
                                        // Replace this line:
                                        // .WithParentActivityOrWindow(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity)

                                        // With the following platform-specific logic:
#if ANDROID
                                            .WithParentActivityOrWindow(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity)
#elif WINDOWS
                                            // On Windows, you can pass null or use a window handle if available.
                                            .WithParentActivityOrWindow(App.Current.MainPage)
#else
                                        // On other platforms, you may need to omit this method or provide the appropriate window/activity.
#endif
                                        .ExecuteAsync()
                                        .ConfigureAwait(false);

                return _authResult?.AccessToken;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during interactive sign-in: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets a valid access token, refreshing silently if needed.
        /// This is for making API calls after the initial login.
        /// </summary>
        public async Task<string> GetAccessTokenAsync()
        {
            await WaitForInitialization().ConfigureAwait(false);
            var accounts = await _pca.GetAccountsAsync().ConfigureAwait(false);
             if (!accounts.Any())
             {
                 return null; // Not logged in
             }

             try
             {
                 _authResult = await _pca.AcquireTokenSilent(EntraIdConstants.Scopes, accounts.FirstOrDefault())
                                         .ExecuteAsync().ConfigureAwait(false);
             }
             catch(MsalUiRequiredException)
             {
                 // This shouldn't happen often in the middle of a session, but if it does, 
                 // you might want to force a re-login.
                 return null; 
             }

             return _authResult.AccessToken;
        }

        /// <summary>
        /// Retrieves the collection of accounts currently known by the application's token cache.
        /// An empty list means no user is logged in.
        /// </summary>
        /// <returns>An enumeration of IAccount objects.</returns>
        public async Task<IEnumerable<IAccount>> GetLoggedInAccountsAsync()
        {
            // The _pca object is your MSAL PublicClientApplication instance.
            // The GetAccountsAsync() method securely queries the device's token cache
            // and returns all user accounts that the app has previously authenticated.
            await WaitForInitialization().ConfigureAwait(false);
            return await _pca.GetAccountsAsync().ConfigureAwait(false);
        }

        public async Task SignOutAsync()
        {
            await WaitForInitialization().ConfigureAwait(false);
            var accounts = await _pca.GetAccountsAsync();
            foreach (var account in accounts)
            {
                await _pca.RemoveAsync(account).ConfigureAwait(false);
            }
        }
    }
}
