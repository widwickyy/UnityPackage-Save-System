using System;
using System.Security.Cryptography;
using System.Text;

namespace Widwickyy.SaveSystem
{
    public class AesStringCipher : IStringCipher
    {
        private const int IvSize = 16;

        private readonly byte[] _key;

        public AesStringCipher(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Encryption key cannot be null or empty.", nameof(key));

            using var sha256 = SHA256.Create();
            _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
        }

        public string Encrypt(string plainText)
        {
            if (plainText == null)
                return null;

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            var plainBytes = Encoding.UTF8.GetBytes(plainText);

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            var payload = new byte[IvSize + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, payload, 0, IvSize);
            Buffer.BlockCopy(cipherBytes, 0, payload, IvSize, cipherBytes.Length);

            return Convert.ToBase64String(payload);
        }

        public string Decrypt(string encryptedText)
        {
            if (encryptedText == null)
                return null;

            var payload = Convert.FromBase64String(encryptedText);
            if (payload.Length <= IvSize)
                throw new CryptographicException("Encrypted payload is invalid.");

            var iv = new byte[IvSize];
            var cipherBytes = new byte[payload.Length - IvSize];

            Buffer.BlockCopy(payload, 0, iv, 0, IvSize);
            Buffer.BlockCopy(payload, IvSize, cipherBytes, 0, cipherBytes.Length);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
