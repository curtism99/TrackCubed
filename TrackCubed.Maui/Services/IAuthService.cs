namespace TrackCubed.Maui.Services
{
    public interface IAuthService
    {
        Task<string?> SilentSignInAsync();
        Task<AuthSession?> InteractiveLoginAsync();
        Task<string?> GetAccessTokenAsync();
        Task SignOutAsync();
        (string Name, string Email) GetCurrentUser();
    }
}
