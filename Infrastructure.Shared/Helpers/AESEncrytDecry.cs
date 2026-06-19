using Application.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Shared.Helpers
{
    public class AESEncrytDecry: IAESEncrytDecry
    {
        private static Random RNG = new Random();
        // 🔐 Secure key generation
        public string GenerateKey()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }

        public string EncryptAES(string plainText, string secret)
        {
            // Derive key + IV exactly like CryptoJS
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(secret));

            var key = hash;                  // 32 bytes (AES-256)
            var iv = hash.Take(16).ToArray(); // 16 bytes

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor();

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(
                plainBytes,
                0,
                plainBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }
        public string DecryptAES(string cipherText, string secret)
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
        public async Task<T> DecryptAESWithDTO<T>(string cipherText, string key)
        {
            try
            {
                string json = DecryptAES(cipherText, key);

                if (string.IsNullOrEmpty(json))
                    return default;

                return await Task.FromResult(JsonConvert.DeserializeObject<T>(json));
            }
            catch
            {
                return default;
            }
        }

    }
}
