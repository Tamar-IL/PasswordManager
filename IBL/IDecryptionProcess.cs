using MyProject.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBL
{
    public interface IDecryptionProcess
    {
        string ConvertAsciiToString(int[] asciiValues);
        int[] AdjacencyMatrixToBlock(int[,] adjacencyMatrix);
        int[,] VectorToMatrix(int[] vector);
        List<int[]> ParseEncryptedMessage(int[] encryptedMessage, int blocksCount);
        int[] GetTextByPrefix(int[] asciiArray);
        string Decrypt(int[] encryptedMessage, List<int> vectorOfPositions);


    }
}
