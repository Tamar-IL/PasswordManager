using IBL;
using IBL.RSAForMasterKey;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public class AESEncryptionBL : IAESEncryptionBL
    {
    
        private readonly string _key;
        private readonly IRSAencryption _rsa;

        public AESEncryptionBL(IConfiguration config, IRSAencryption rsa)
        {
            _key = config["Encryption:AESKey"] ?? throw new Exception("AES key not found");
            _rsa = rsa;
            // ודא שהמפתח באורך נכון
            if (_key.Length != 32)
                throw new Exception("AES key must be 32 characters");
        }

        public string Encrypt(string plaintext)
        {
            if (string.IsNullOrEmpty(plaintext)) return null;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_key);
                aes.GenerateIV(); // IV אקראי בכל הצפנה

                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

                    // צרף IV לתחילת הנתונים המוצפנים
                    byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
                    Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                    Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

                    return Convert.ToBase64String(result);
                }
            }
        }

        public string Decrypt(string ciphertext)
        {
            if (string.IsNullOrEmpty(ciphertext)) return null;

            byte[] ciphertextBytes = Convert.FromBase64String(ciphertext);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_key);

                // חלץ את ה-IV מהתחילה
                byte[] iv = new byte[16];
                Array.Copy(ciphertextBytes, 0, iv, 0, 16);
                aes.IV = iv;

                // חלץ את הנתונים המוצפנים
                byte[] encryptedData = new byte[ciphertextBytes.Length - 16];
                Array.Copy(ciphertextBytes, 16, encryptedData, 0, encryptedData.Length);

                using (var decryptor = aes.CreateDecryptor())
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }

        public string getKey()
        {
            return _key;
        }
    }
}

