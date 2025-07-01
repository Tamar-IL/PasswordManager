using MyProject.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MyProject.Common;

namespace IBL
{
    public interface IgenerateKeyEncrryption
    {
        int[] GenerateSeed(int initialValue);
        (int[][,] SubKeys, List<int> VectorOfPositions) GenerateSubKeysForEncryption(List<int[]> blocks);
    }
}
