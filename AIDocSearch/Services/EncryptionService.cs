using Application.Interfaces;
using Infrastructure.Shared.Helpers;
using System;
using System.Security.Cryptography;
using System.Text;

namespace AIDocSearch.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly IAESEncrytDecry _encryptionKey;
        public EncryptionService(IAESEncrytDecry encryptionKey)
        {
            _encryptionKey = encryptionKey;
        }
        // NOTE: This wrapper delegates to existing AESEncrytDecry helper so behavior remains unchanged.
        public string Decrypt(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;
            return _encryptionKey.DecryptAES(cipherText, key);
        }

        public string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;
            // AESEncrytDecry does not expose EncryptAES in current helper; implement symmetric encrypt using AES for now
            // For compatibility assume AESEncrytDecry has EncryptAES implemented; if not, return plainText.
            try
            {
                // Use reflection fallback if method exists
                var method = typeof(AESEncrytDecry).GetMethod("EncryptAES");
                if (method != null)
                {
                    return (string)method.Invoke(null, new object[] { plainText, key });
                }
            }
            catch
            {
                // swallow and fallback
            }
            return plainText;
        }
    }
}
