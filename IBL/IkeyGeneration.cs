using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IBL
{
    public interface IkeyGeneration
    {
        Task<List<int>> ConvertMessage(string message, int n);
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
