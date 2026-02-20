using System.Security.Cryptography;
using System.Text;

namespace AIDocSearch.Helpers
{
    public static class AESEncrytDecry
    {
        // 🔐 Secure key generation
        public static string GenerateKey()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }

        public static string DecryptAES(string cipherText, string secret)
        {
            // Derive key + IV exactly like CryptoJS
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(secret));

            var key = hash;                 // 32 bytes (AES-256)
            var iv = hash.Take(16).ToArray(); // 16 bytes

            var buffer = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var decrypted = decryptor.TransformFinalBlock(buffer, 0, buffer.Length);

            return Encoding.UTF8.GetString(decrypted);
        }
    }
}