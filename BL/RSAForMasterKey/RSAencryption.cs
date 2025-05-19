using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using IBL.RSAForMasterKey;

namespace BL.RSAForMasterKay
{
    public class RSAencryption:IRSAencryption
    {
        private readonly string _privateKeyPath;
        public RSAencryption(string privateKeyPath)
        {
            _privateKeyPath = privateKeyPath;
        }
        public (string PublicKey,string PrivateKey) GeneratePairKey(int keySize = 2048)
        {
            using (RSA rsa = RSA.Create(keySize))
            {
                return (rsa.ToXmlString(false), rsa.ToXmlString(true));
            }

        }
        public byte[] Encrypt(string plainText,string PublicKey)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.FromXmlString(PublicKey);
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                return rsa.Encrypt(plainBytes, RSAEncryptionPadding.OaepSHA256);
            }
        }
        public string Decrypt(byte[] encryptData ,string privateKay)
        {
            using(RSA rsa = RSA.Create())
            {
                rsa.FromXmlString(privateKay);
                byte[] decryptedBytes = rsa.Decrypt(encryptData, RSAEncryptionPadding.OaepSHA256);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
        public string Decrypt(byte[] encryptData)
        {
            string privateKey = GetPrivateKayFromSecureStorge();
            return Decrypt(encryptData, privateKey);
        }
        private string GetPrivateKayFromSecureStorge()
        {
            if (System.IO.File.Exists(_privateKeyPath))
            {
                return System.IO.File.ReadAllText(_privateKeyPath);
            }

            throw new FileNotFoundException("the private not found", _privateKeyPath);
        }


    }
}
