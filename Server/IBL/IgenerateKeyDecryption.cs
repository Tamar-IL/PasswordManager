using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBL
{
    public interface IgenerateKeyDecryption
    {
        List<int[]> ParseKey(int[] key, int blocksCount);
        int[] GenerateKey(int[] keyEncryptionKey, List<int> vectorOfPositions);
        int[][,] GenerateSubKeysForDecryption(List<int> vectorOfPositions);
    }
}
