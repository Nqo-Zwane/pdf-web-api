using System;
using System.Security.Cryptography;

namespace WebApi.utils
{    public class SecretKeyGenerator
    {
        public static string GenerateSecretKey()
        {
            var randomBytes = new byte[32]; // 256 bits
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
    }
}
