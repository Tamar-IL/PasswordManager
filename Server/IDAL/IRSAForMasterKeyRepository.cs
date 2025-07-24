using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IRSAForMasterKeyRepository
    {
        Task< string> GetPrivateKayFromSecureStorge();
       Task <string> Decrypt(byte[] encryptData);
        Task<string> Decrypt(byte[] encryptData, string privateKay);
        Task<byte[]> Encrypt(string plainText, string PublicKey);
        Task<(string PublicKey, string PrivateKey)>GeneratePairKey(int keySize = 2048);

    }
}
