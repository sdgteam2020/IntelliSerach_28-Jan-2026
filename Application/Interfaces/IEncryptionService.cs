namespace Application.Interfaces
{
    public interface IEncryptionService
    {
        string Decrypt(string cipherText, string key);
        string Encrypt(string plainText, string key);
    }
}
