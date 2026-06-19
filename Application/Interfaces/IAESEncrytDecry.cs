using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IAESEncrytDecry
    {
        public string GenerateKey();
        public string DecryptAES(string cipherText, string secret);
        public string EncryptAES(string plainText, string secret);
        public Task<T> DecryptAESWithDTO<T>(string cipherText, string key);
    }
}
