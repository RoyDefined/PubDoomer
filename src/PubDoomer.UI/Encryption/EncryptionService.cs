using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Encryption;

public sealed class EncryptionService
{
    private readonly byte[] _key;

    public EncryptionService()
    {
        using var sha256 = SHA256.Create();
        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes("TODO"));
    }

    public async Task<byte[]> EncryptAsync(string plainText)
    {
        using var aes = Aes.Create();
        aes.GenerateIV();
        aes.Key = _key;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);

        await using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);

        // The writer is scoped because this ensures that all bytes are written before disposing.
        await using (var sw = new StreamWriter(cs))
        {
            await sw.WriteAsync(plainText);
        }

        return ms.ToArray();
    }

    public async Task<string> DecryptAsync(byte[] encryptedBytes)
    {
        using var aes = Aes.Create();
        aes.Key = _key;

        var iv = new byte[aes.BlockSize / 8];
        Array.Copy(encryptedBytes, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(encryptedBytes, iv.Length, encryptedBytes.Length - iv.Length);
        await using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return await sr.ReadToEndAsync();
    }
}