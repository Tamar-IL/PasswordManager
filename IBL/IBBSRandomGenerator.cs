using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace IBL
{
    public interface IBBSRandomGenerator
    {
       
        BigInteger GenerateSeed(BigInteger s);
        Task<int> Next(int max);
        bool IsPrime(BigInteger number);

    }
}
