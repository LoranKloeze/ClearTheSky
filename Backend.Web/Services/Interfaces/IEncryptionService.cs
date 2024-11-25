namespace Backend.Web.Services.Interfaces;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}