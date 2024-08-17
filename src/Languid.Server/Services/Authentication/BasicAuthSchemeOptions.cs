using Microsoft.AspNetCore.Authentication;

namespace Languid.Server.Authentication
{
    public class BasicAuthSchemeOptions : AuthenticationSchemeOptions
    {
        public BasicAuthSchemeOptions()
        {
            ServerUsername = "";
            HashedPassword = "";
            Salt = "";
        }
        public string ServerUsername { get; set; }
        public string HashedPassword { get; set; }
        public string Salt { get; set; }
    }
}