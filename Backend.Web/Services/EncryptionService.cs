using Backend.Web.Services.Interfaces;

namespace Backend.Web.Services;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(string key)
    {
        if (key.Length != 32)
            throw new ArgumentException("Encryption key must be 32 characters long.");
        
        _key = Encoding.UTF8.GetBytes(key);
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        var iv = aes.IV;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var writer = new StreamWriter(cs))
        {
            writer.Write(plainText);
        }

        var encrypted = ms.ToArray();
        var result = Convert.ToBase64String(iv) + ":" + Convert.ToBase64String(encrypted);
        return result;
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        var parts = cipherText.Split(':');
        if (parts.Length != 2)
            throw new FormatException("Invalid encrypted text format.");

        var iv = Convert.FromBase64String(parts[0]);
        var cipherBytes = Convert.FromBase64String(parts[1]);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(cipherBytes);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs);
        return reader.ReadToEnd();
    }
}
