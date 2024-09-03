using System.Security.Cryptography;
using System.Text;

public interface IEncryptionHelper{
    public byte[] Encrypt(string text);
    public string Decrypt(byte[] cipherText);
}

public class EncryptionHelper : IEncryptionHelper
{
    private readonly byte[] Key; // 32 bytes for AES-256

    public EncryptionHelper(string key)
    {
        Key = Encoding.UTF8.GetBytes(key);
    }

    public byte[] Encrypt(string text)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = Key;
        aesAlg.GenerateIV(); // Generate a new IV for each encryption

        var iv = aesAlg.IV; // Get the generated IV
        var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv);

        using var msEncrypt = new MemoryStream();
        // Prepend the IV to the encrypted message
        msEncrypt.Write(iv, 0, iv.Length);

        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(text);
        }

        return msEncrypt.ToArray();
    }

    public string Decrypt(byte[] cipherText)
    {
        using var aesAlg = Aes.Create();
        using var msDecrypt = new MemoryStream(cipherText);
        // Read the IV from the beginning of the encrypted message
        
        var iv = new byte[16];
        msDecrypt.Read(iv, 0, iv.Length);

        aesAlg.Key = Key;
        aesAlg.IV = iv;

        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
        using (var srDecrypt = new StreamReader(csDecrypt))
        {
            return srDecrypt.ReadToEnd();
        }
    }
}