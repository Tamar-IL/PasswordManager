using AutoMapper;
using IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class RSAForMasterKeyRepository:IRSAForMasterKeyRepository
    {
        private readonly MongoDbService _dbService;
        private readonly IMapper _mapper;
        public RSAForMasterKeyRepository(MongoDbService dbService, IMapper mapper)  
        {
            _dbService = dbService;
            _mapper = mapper;
        }

        public Task<string> Decrypt(byte[] encryptData)
        {
            throw new NotImplementedException();
        }

        public Task<string> Decrypt(byte[] encryptData, string privateKay)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> Encrypt(string plainText, string PublicKey)
        {
            throw new NotImplementedException();
        }

        public Task<(string PublicKey, string PrivateKey)> GeneratePairKey(int keySize = 2048)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPrivateKayFromSecureStorge()
        {
            throw new NotImplementedException();
        }
    }
}
