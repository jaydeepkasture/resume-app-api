using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;

namespace ResumeInOneMinute.Infrastructure.CommonServices;

/// <summary>
/// Encryption helper service providing both temporary and persistent encryption methods
/// </summary>
public class EncryptionHelper
{
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IDataProtector _dataProtector;
    private const string EncryptionKey = "ResumeInOneMinute_PersistentEncryption_Key_2026"; // Should be from config in production

    public EncryptionHelper(IDataProtectionProvider dataProtectionProvider)
    {
        _dataProtectionProvider = dataProtectionProvider;
        _dataProtector = _dataProtectionProvider.CreateProtector("ResumeInOneMinute.TemporaryData");
    }

    #region Temporary Encryption (Data Protection API)

    /// <summary>
    /// Encrypts data temporarily using Microsoft Data Protection API
    /// Use this for short-lived data like tokens, session data, etc.
    /// </summary>
    /// <param name="plainText">Text to encrypt</param>
    /// <returns>Encrypted string</returns>
    public string EncryptTemporary(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentNullException(nameof(plainText));

        return _dataProtector.Protect(plainText);
    }

    /// <summary>
    /// Decrypts data that was encrypted using EncryptTemporary
    /// </summary>
    /// <param name="encryptedText">Encrypted text</param>
    /// <returns>Decrypted string</returns>
    public string DecryptTemporary(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            throw new ArgumentNullException(nameof(encryptedText));

        return _dataProtector.Unprotect(encryptedText);
    }

    #endregion

    #region Persistent Encryption (Custom AES)

    /// <summary>
    /// Encrypts data for persistent storage using AES-256
    /// Use this for data that needs to be stored in database and decrypted later
    /// </summary>
    /// <param name="plainText">Text to encrypt</param>
    /// <returns>Base64 encoded encrypted string</returns>
    public string EncryptPersistent(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentNullException(nameof(plainText));

        using (var aes = Aes.Create())
        {
            aes.Key = DeriveKeyFromPassword(EncryptionKey);
            aes.GenerateIV();

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (var msEncrypt = new MemoryStream())
            {
                // Write IV to the beginning of the stream
                msEncrypt.Write(aes.IV, 0, aes.IV.Length);

                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }

                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    /// <summary>
    /// Decrypts data that was encrypted using EncryptPersistent
    /// </summary>
    /// <param name="encryptedText">Base64 encoded encrypted text</param>
    /// <returns>Decrypted string</returns>
    public string DecryptPersistent(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            throw new ArgumentNullException(nameof(encryptedText));

        var fullCipher = Convert.FromBase64String(encryptedText);

        using (var aes = Aes.Create())
        {
            aes.Key = DeriveKeyFromPassword(EncryptionKey);

            // Extract IV from the beginning of the encrypted data
            var iv = new byte[aes.IV.Length];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);
            aes.IV = iv;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (var msDecrypt = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (var srDecrypt = new StreamReader(csDecrypt))
            {
                return srDecrypt.ReadToEnd();
            }
        }
    }

    /// <summary>
    /// Derives a 256-bit key from a password using PBKDF2
    /// </summary>
    private byte[] DeriveKeyFromPassword(string password)
    {
        var salt = Encoding.UTF8.GetBytes("ResumeInOneMinute_Salt_2026"); // Should be from config in production
        using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
        {
            return deriveBytes.GetBytes(32); // 256 bits
        }
    }

    #endregion

    #region Hash Functions (One-way)

    /// <summary>
    /// Creates a SHA-256 hash of the input (one-way, cannot be decrypted)
    /// Use for password hashing, data integrity checks, etc.
    /// </summary>
    /// <param name="input">Text to hash</param>
    /// <returns>Base64 encoded hash</returns>
    public string CreateHash(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentNullException(nameof(input));

        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

    /// <summary>
    /// Verifies if input matches a previously created hash
    /// </summary>
    /// <param name="input">Text to verify</param>
    /// <param name="hash">Previously created hash</param>
    /// <returns>True if input matches hash</returns>
    public bool VerifyHash(string input, string hash)
    {
        var inputHash = CreateHash(input);
        return inputHash == hash;
    }

    #endregion
}
