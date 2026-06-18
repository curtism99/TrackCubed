using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace TrackCubed.Api.Auth
{
    public sealed class DevelopmentAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "TrackCubedDevelopment";

        public DevelopmentAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "trackcubed-local-dev-user"),
                new Claim("oid", "trackcubed-local-dev-user"),
                new Claim("name", "Local TrackCubed Developer"),
                new Claim(ClaimTypes.Email, "local.dev@trackcubed.test"),
                new Claim("preferred_username", "local.dev@trackcubed.test"),
                new Claim("scp", "CubedItems.ReadWrite"),
                new Claim("http://schemas.microsoft.com/identity/claims/scope", "CubedItems.ReadWrite")
            };

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
