using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Languid.Server.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Languid.Server.Authentication
{
    public class BasicAuthSchemeHandler : AuthenticationHandler<BasicAuthSchemeOptions>
    {
        public BasicAuthSchemeHandler(
            IOptionsMonitor<BasicAuthSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
                : base(options, logger, encoder)
        {}

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            (var isAuthorizationHeaderValid, var username, var password) = TryParseAuthHeader(Context.Request.Headers);

            if (!isAuthorizationHeaderValid || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return Task.FromResult(AuthenticateResult.Fail("Authentication failed."));
            }

            if (username != Options.ServerUsername)
            {
                return Task.FromResult(AuthenticateResult.Fail("Authentication failed."));
            }

            if (PasswordHasher.HashPassword(password, Options.Salt) != Options.HashedPassword)
            {
                return Task.FromResult(AuthenticateResult.Fail("Authentication failed."));
            }

            var claims = new [] { new Claim(ClaimTypes.Name, username)};
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Basic"));
            var ticket = new AuthenticationTicket(principal, this.Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        private Tuple<bool, string, string> TryParseAuthHeader(IHeaderDictionary headers)
        {
            if (!headers.ContainsKey("Authorization"))
            {
                return new Tuple<bool, string, string>(false, "", "");
            }

            var authorizationHeader = headers["Authorization"];
            var authorizationHeaderValue = authorizationHeader.FirstOrDefault();

            if (string.IsNullOrEmpty(authorizationHeaderValue))
            {
                return new Tuple<bool, string, string>(false, "", "");
            }

            var authorizationHeaderParts = authorizationHeaderValue.Split(' ');

            if (authorizationHeaderParts.Length < 2)
            {
                return new Tuple<bool, string, string>(false, "", "");
            }
            var base64UsernamePassword = authorizationHeaderParts[1];

            var usernamePasswordParts = Encoding.UTF8.GetString(Convert.FromBase64String(base64UsernamePassword)).Split(':');

            if (usernamePasswordParts.Length < 2)
            {
                return new Tuple<bool, string, string>(false, "", "");
            }

            return new Tuple<bool, string, string>(true, usernamePasswordParts[0], usernamePasswordParts[1]);
        }
    }
}