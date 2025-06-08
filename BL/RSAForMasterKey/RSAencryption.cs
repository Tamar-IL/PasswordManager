using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using IBL.RSAForMasterKey;
using System.Security.Cryptography.X509Certificates;

namespace BL.RSAForMasterKay
{
    public class RSAencryption : IRSAencryption
    {
        private readonly string _privateKeyPath;
        private string _publicKey;
        private string _privateKey;

        public RSAencryption(string privateKeyPath)
        {
            _privateKeyPath = privateKeyPath;
            InitializeKeys();
        }

        private void InitializeKeys()
        {
            if (File.Exists(_privateKeyPath))
            {
                // טוען מפתחות קיימים מהקובץ
                LoadExistingKeys();
            }
            else
            {
                // יוצר מפתחות חדשים ושומרר אותם
                GenerateAndSaveNewKeys();
            }
        }

        private void LoadExistingKeys()
        {
            try
            {
                _privateKey = File.ReadAllText(_privateKeyPath, Encoding.UTF8);

                using (RSA rsa = RSA.Create())
                {
                    rsa.FromXmlString(_privateKey);
                    _publicKey = rsa.ToXmlString(false); // מחלץ את המפתח הציבורי מהפרטי
                }

                Console.WriteLine("Keys loaded successfully from file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading existing keys: {ex.Message}");
                //// אם יש בעיה בטעינת המפתחות הקיימים, ליצור חדשים
                //GenerateAndSaveNewKeys();
            }
        }

        private void GenerateAndSaveNewKeys()
        {
            try
            {
                using (RSA rsa = RSA.Create(2048))
                {
                    _privateKey = rsa.ToXmlString(true);
                    _publicKey = rsa.ToXmlString(false);

                    // וודא שהתיקייה קיימת
                    string directory = Path.GetDirectoryName(_privateKeyPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllText(_privateKeyPath, _privateKey, Encoding.UTF8);
                    Console.WriteLine("New keys generated and saved successfully.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate and save new keys: {ex.Message}", ex);
            }
        }

        public string GetPublicKey()
        {
            return _publicKey;
        }

        public (string PublicKey, string PrivateKey) GeneratePairKey(int keySize = 2048)
        {
            using (RSA rsa = RSA.Create(keySize))
            {
                string privateKey = rsa.ToXmlString(true);
                string publicKey = rsa.ToXmlString(false);

                // שמור את המפתחות החדשים
                _privateKey = privateKey;
                _publicKey = publicKey;

                File.WriteAllText(_privateKeyPath, privateKey, Encoding.UTF8);
                return (publicKey, privateKey);
            }
        }

        public byte[] Encrypt(string plainText, string publicKey)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentException("Plain text cannot be null or empty");

            if (string.IsNullOrEmpty(publicKey))
                throw new ArgumentException("Public key cannot be null or empty");

            try
            {
                using (RSA rsa = RSA.Create())
                {
                    rsa.FromXmlString(publicKey);
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    return rsa.Encrypt(plainBytes, RSAEncryptionPadding.Pkcs1);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Encryption failed: {ex.Message}", ex);
            }
        }

        public string Decrypt(byte[] encryptData, string privateKey)
        {
            if (encryptData == null || encryptData.Length == 0)
                throw new ArgumentException("Encrypted data cannot be null or empty");

            if (string.IsNullOrEmpty(privateKey))
                throw new ArgumentException("Private key cannot be null or empty");

            try
            {
                using (RSA rsa = RSA.Create())
                {
                    rsa.FromXmlString(privateKey);
                    byte[] decryptedBytes = rsa.Decrypt(encryptData, RSAEncryptionPadding.Pkcs1);

                    if (decryptedBytes == null || decryptedBytes.Length == 0)
                    {
                        throw new Exception("Decryption resulted in empty data");
                    }

                    string result = Encoding.UTF8.GetString(decryptedBytes);
                    Console.WriteLine($"Decryption successful. Result length: {result.Length}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Decryption failed: {ex.Message}", ex);
            }
        }

        public string Decrypt(byte[] encryptData)
        {
            string privateKey = GetPrivateKayFromSecureStorge();
            return Decrypt(encryptData, privateKey);
        }

        public string GetPrivateKayFromSecureStorge()
        {
            if (string.IsNullOrEmpty(_privateKey))
            {
                if (File.Exists(_privateKeyPath))
                {
                    _privateKey = File.ReadAllText(_privateKeyPath, Encoding.UTF8);
                }
                else
                {
                    throw new FileNotFoundException("Private key file not found", _privateKeyPath);
                }
            }
            return _privateKey;
        }
    }
}