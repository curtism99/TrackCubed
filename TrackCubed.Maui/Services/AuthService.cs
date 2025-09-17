using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackCubed.Maui.Configs;
using System.Security.Claims;

namespace TrackCubed.Maui.Services
{
       public class AuthService
    {
        private IPublicClientApplication _pca;
        private AuthenticationResult _authResult;

        // This object acts as the "gatekeeper" for our initialization.
        // It holds a Task that we can await. The task only completes when we tell it to.
        private readonly TaskCompletionSource<bool> _initializationComplete = new TaskCompletionSource<bool>();

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

            try
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

#endif

            // Initialization is done. Set the result to true to "open the gate".
            // Any code awaiting our task will now unblock and continue.
            _initializationComplete.SetResult(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FATAL: MSAL client initialization failed: {ex.Message}");

                // *** THE FIX: Part 3 - Signal Failure ***
                // Something went wrong. Signal the failure.
                _initializationComplete.SetException(ex);
            }
        }


        // This is the new, correct implementation. It simply awaits the single task.
        // No loops, no polling, no wasted CPU.
        private Task WaitForInitialization()
        {
            return _initializationComplete.Task;
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
                    // Assign the result to our private field so GetCurrentUser works after a silent sign-in
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
        public async Task<AuthenticationResult> InteractiveLoginAsync()
        {
            await WaitForInitialization();
            try
            {
                // 1. Get all accounts MSAL already knows about for this app.
                var accounts = await _pca.GetAccountsAsync().ConfigureAwait(false);
                IAccount firstAccount = accounts.FirstOrDefault();

                // 2. BEST PRACTICE: Try to get a token silently first.
                //    If successful, the user avoids the login prompt entirely.
                if (firstAccount != null)
                {
                    try
                    {
                        // This will use the cached refresh token to get a new access token.
                        return await _pca.AcquireTokenSilent(EntraIdConstants.Scopes, firstAccount)
                                         .ExecuteAsync()
                                         .ConfigureAwait(false);
                    }
                    catch (MsalUiRequiredException)
                    {
                        // This is not an error. It's an expected condition if the token has expired
                        // or consent is needed. We'll fall through to the interactive login.
                        System.Diagnostics.Debug.WriteLine("Silent token acquisition failed. UI is required.");
                    }
                }

                // 3. If silent auth fails or no account exists, proceed with interactive login.
                //    We create a "builder" to conditionally add our parameters.
                var interactiveRequestBuilder = _pca.AcquireTokenInteractive(EntraIdConstants.Scopes);

                // 4. THIS IS THE FIX: If we have a known account, use it.
                //    This provides the best "Welcome back" experience and avoids the "mailto:" link issue.
                if (firstAccount != null)
                {
                    interactiveRequestBuilder.WithAccount(firstAccount);
                }

                // 5. YOUR PLATFORM LOGIC IS PRESERVED HERE:
                //    We add the platform-specific parent window/activity to the builder.
#if ANDROID
                interactiveRequestBuilder.WithParentActivityOrWindow(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity);
#elif WINDOWS
        // Note: Using App.Current.MainPage might work, but the official recommendation
        // is often a window handle (IntPtr). If this works for you, it's fine.
        interactiveRequestBuilder.WithParentActivityOrWindow(App.Current.MainPage); 
#endif
                // On other platforms (iOS, MacCatalyst), .WithParentActivityOrWindow is often not needed
                // as MSAL can infer the top view controller.

                // 6. Execute the interactive request we have built.
                _authResult = await interactiveRequestBuilder.ExecuteAsync()
                                                             .ConfigureAwait(false);

                // CRITICAL FIX: Return the entire AuthenticationResult object.
                // The calling code needs the full result to access the IAccount for the next silent call,
                // not just the access token string.
                return _authResult;
            }
            catch (MsalClientException ex)
            {
                // Specifically handle cases where the user cancels the login prompt.
                if (ex.ErrorCode == "authentication_canceled")
                {
                    System.Diagnostics.Debug.WriteLine("User cancelled the sign-in.");
                    return null;
                }
                // Handle other client-side MSAL errors.
                System.Diagnostics.Debug.WriteLine($"MSAL Client Error during interactive sign-in: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Generic Error during interactive sign-in: {ex.Message}");
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

        public (string Name, string Email) GetCurrentUser()
        {
            // _authResult is the AuthenticationResult from the last successful sign-in
            if (_authResult?.ClaimsPrincipal == null)
            {
                return ("Not signed in", string.Empty);
            }

            // Extract the user's name and preferred username (email) from the claims principal
            var name = _authResult.ClaimsPrincipal.FindFirst("name")?.Value ?? "N/A";
            var email = _authResult.ClaimsPrincipal.FindFirst("preferred_username")?.Value ?? "N/A";

            return (name, email);
        }
    }
}
