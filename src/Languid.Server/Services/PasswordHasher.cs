using System.Security.Cryptography;
using System.Text;

namespace Languid.Server.Services
{
    public static class PasswordHasher
    {
        public static string HashPassword(string pwd, string salt)
        {
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes($"{pwd}{salt}")));
        }
    }
}