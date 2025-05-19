using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBL.RSAForMasterKey
{
    public interface IRSAencryption
    {
        (string PublicKey, string PrivateKey) GeneratePairKey(int keySize = 2048);
        byte[] Encrypt(string PlainText, string PublicKey);
        string Decrypt(byte[] encryptData, string privateKey);
        string Decrypt(byte[] encryptData);

    }
}
