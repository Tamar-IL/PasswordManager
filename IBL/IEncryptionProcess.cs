using MyProject.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IBL
{
    public interface IEncryptionProcess
    {
        string AddLengthAsPrefix(string input);
        string AddSaltToMessageEnd(string message);
        int[] ConvertMessageToAscii(string message);
        List<int[]> ParseMessage(int[] message, int blocksCount);
        int[] MatrixToVector(int[,] matrix);
        int[] ConvertBytesToInts(byte[] bytes);
        int[,] BlockToAdjacencyMatrix(List<int[]> subBlocks);
        List<int[]> ParseBlock(int[] block);
        (int[] EncryptedMessage, List<int> VectorOfPositions) Encrypt(string clearMessage);















    }
}
