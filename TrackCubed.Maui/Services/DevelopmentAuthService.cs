namespace TrackCubed.Maui.Services
{
    public sealed class DevelopmentAuthService : IAuthService
    {
        private const string DevToken = "trackcubed-local-dev-token";
        private static readonly AuthSession DevSession = new(
            DevToken,
            "Local TrackCubed Developer",
            "local.dev@trackcubed.test");

        public Task<string?> SilentSignInAsync()
        {
            return Task.FromResult<string?>(DevToken);
        }

        public Task<AuthSession?> InteractiveLoginAsync()
        {
            return Task.FromResult<AuthSession?>(DevSession);
        }

        public Task<string?> GetAccessTokenAsync()
        {
            return Task.FromResult<string?>(DevToken);
        }

        public Task SignOutAsync()
        {
            return Task.CompletedTask;
        }

        public (string Name, string Email) GetCurrentUser()
        {
            return (DevSession.Name, DevSession.Email);
        }
    }
}
