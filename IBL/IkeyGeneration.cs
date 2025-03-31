using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace IBL
{
    interface IkeyGeneration
    {
        Task<List<int>> ConvertMessage(String message, int n);
        Task<List<List<int>>> parseMessage(List<int> cma, int k);
        Task<int> getCharFromBlock(List<int> Blocki);
        void addToVP(int c);
        Task<int> getNbrFromKEK(List<int> KEK, int p);
        Task<int[]> generateSeed(BigInteger n);
        Task<int[][]> generateSubKey(int[] si);
        Task putSubKey(int[][] subKey);
        Task<(List<int[][]>, List<int>)> generateKey(string message, int k, List<int> KEK);

    }
}
