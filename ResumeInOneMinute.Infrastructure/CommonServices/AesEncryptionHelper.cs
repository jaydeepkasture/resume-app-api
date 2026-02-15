using System.Security.Cryptography;
using System.Text;

namespace ResumeInOneMinute.Infrastructure.CommonServices;

/// <summary>
/// Helper for AES encryption compatible with CryptoJS (Angular)
/// </summary>
public static class AesEncryptionHelper
{
    // MUST be 32 characters for AES-256
    private const string KeyString = "ResumeInOneMinute_AES_Key_2026_!"; 
    // MUST be 16 characters
    private const string IvString = "1234567890123456"; 

    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        byte[] key = Encoding.UTF8.GetBytes(KeyString);
        byte[] iv = Encoding.UTF8.GetBytes(IvString);

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7; // PKCS7 matches CryptoJS default

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                }
                // Return as Base64 so it can be sent via JSON/API
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;

        byte[] key = Encoding.UTF8.GetBytes(KeyString);
        byte[] iv = Encoding.UTF8.GetBytes(IvString);

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
}
