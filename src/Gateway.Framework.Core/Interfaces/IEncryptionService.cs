namespace Gateway.Framework.Core.Interfaces;

/// <summary>
/// Interface for encryption operations on sensitive data.
/// </summary>
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    string Hash(string input);
    bool VerifyHash(string input, string hash);
}
