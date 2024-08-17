using System.Text;
using Languid.Server.Authentication;
using Microsoft.AspNetCore.Authentication;

namespace Languid.Server.Services
{
    public static class AppBuilderExtensions
    {
        public static AuthenticationBuilder AddBasicAuthentication(this AuthenticationBuilder builder)
        {
            return builder.AddScheme<BasicAuthSchemeOptions, BasicAuthSchemeHandler>("Basic", options => {
                var configuration = builder.Services.BuildServiceProvider().GetRequiredService<IConfiguration>();

                if (configuration == null)
                {
                    throw new ArgumentNullException("Can not load configuration for authentication scheme.");
                }

                options.ServerUsername = configuration["USERNAME"] ?? throw new ArgumentNullException("Server username is not configured.");
                options.HashedPassword = configuration["HASHED_PASSWORD"] ?? throw new ArgumentNullException("Hashed password is not configured");
                options.Salt = configuration["SALT"] ?? "";
            });
        }
    }
}