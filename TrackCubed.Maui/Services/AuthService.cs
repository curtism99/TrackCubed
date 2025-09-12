using Microsoft.Identity.Client;
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
        private readonly IPublicClientApplication _pca;
        private AuthenticationResult _authResult;

        public AuthService()
        {
            // The builder is smart enough to use the correct redirect URI 
            // for the platform it's running on (Windows, Android, or iOS)
            // as long as they are all registered in the Azure portal.
            _pca = PublicClientApplicationBuilder.Create(EntraIdConstants.ClientId)
                        .WithTenantId(EntraIdConstants.TenantId)
                        .WithDefaultRedirectUri() // This is a helper that resolves to localhost on desktop and is not needed on mobile
                        .Build();
        }

        public async Task<string> LoginAsync()
        {
            try
            {
                // Attempt to get a token silently first
                var accounts = await _pca.GetAccountsAsync();
                _authResult = await _pca.AcquireTokenSilent(EntraIdConstants.Scopes, accounts.FirstOrDefault())
                                        .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                // Silent failed, needs interactive login
                _authResult = await _pca.AcquireTokenInteractive(EntraIdConstants.Scopes)
                                        .WithParentActivityOrWindow(App.Current.MainPage) // For Android/Windows
                                        .ExecuteAsync();
            }

            return _authResult?.AccessToken;
        }

        // This is the method you will call for every subsequent API request
        public async Task<string> GetAccessTokenAsync()
        {
            var accounts = await _pca.GetAccountsAsync();
            try
            {
                _authResult = await _pca.AcquireTokenSilent(EntraIdConstants.Scopes, accounts.FirstOrDefault())
                                        .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                // Handle case where session has expired and user needs to re-login interactively
                return await LoginAsync();
            }

            return _authResult.AccessToken;
        }
    }
}
