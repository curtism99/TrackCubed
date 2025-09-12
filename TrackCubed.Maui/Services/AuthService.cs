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
        private MsalCacheHelper _cacheHelper;

        public AuthService()
        {
            // This setup MUST be done asynchronously. We call a helper to do this.
            // This is one of the few acceptable use cases for "async void" in a constructor.
            InitializeMsalClientAsync();
        }


        private async void InitializeMsalClientAsync()
        {
            // 1. Define the properties for the cache file.
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
        }

        /// <summary>
        /// Attempts to sign in the user silently using the cached token.
        /// </summary>
        /// <returns>An access token if successful, otherwise null.</returns>
        public async Task<string> SilentSignInAsync()
        {
            try
            {
                var accounts = await _pca.GetAccountsAsync();
                if (accounts.Any())
                {
                    _authResult = await _pca.AcquireTokenSilent(EntraIdConstants.Scopes, accounts.FirstOrDefault())
                                            .ExecuteAsync();
                    
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
            try
            {
                _authResult = await _pca.AcquireTokenInteractive(EntraIdConstants.Scopes)
                                        .WithParentActivityOrWindow(App.Current.MainPage)
                                        .ExecuteAsync();

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
             var accounts = await _pca.GetAccountsAsync();
             if (!accounts.Any())
             {
                 return null; // Not logged in
             }

             try
             {
                 _authResult = await _pca.AcquireTokenSilent(EntraIdConstants.Scopes, accounts.FirstOrDefault())
                                         .ExecuteAsync();
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
            return await _pca.GetAccountsAsync();
        }

        public async Task SignOutAsync()
        {
            var accounts = await _pca.GetAccountsAsync();
            foreach (var account in accounts)
            {
                await _pca.RemoveAsync(account);
            }
        }
    }
}
