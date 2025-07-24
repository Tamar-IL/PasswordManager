using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBL
{
    public interface IAESEncryptionBL
    {
        string Encrypt(string plaintext);
        string Decrypt(string ciphertext);
        string getKey();
    }
}
