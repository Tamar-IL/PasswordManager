using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBL
{
    public interface IgenerateKeyDecryption
    {
        int[] GenerateBBSSequence(int seed, int length);
        int[,] GenerateSubKey(int[] seedVector);
        List<int[]> ParseKey(int[] key, int blocksCount);
        int[] GenerateKey(int[] keyEncryptionKey, List<int> vectorOfPositions);
        int[][,] GenerateSubKeysForDecryption(List<int> vectorOfPositions);

    }
}
