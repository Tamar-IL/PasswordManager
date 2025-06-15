using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using IBL;
using MyProject.Common;



namespace BL
{
    public static  class BBSRandomGenerator 
    {
        private static readonly BigInteger P = BigInteger.Parse("98799821657648109045695379286138768173");
        private static readonly BigInteger Q = BigInteger.Parse("65147279015126562838267191403654601389");
        private static readonly BigInteger M = P * Q;
        public static int[] GenerateBBSSequence(int seed, int length,MySetting _setting)
        {
           

            int[] sequence = new int[length];

            // הבטחה שהזרע הוא חיובי
            BigInteger x = new BigInteger(Math.Abs(seed));
            x = (x * x) % M;  // x₀ = seed² mod m

            for (int i = 0; i < length; i++)
            {
                x = (x * x) % M;  // xₙ₊₁ = xₙ² mod m
                sequence[i] = (int)(x % 256);  // מיפוי לטווח 0-255
            }

            return sequence;
        }

        public static int[,] GenerateSubKey(int[] seedVector, MySetting _setting)
        {
            int[,] subKey = new int[_setting.graphOrder, _setting.graphOrder];

            for (int i = 0; i < _setting.graphOrder; i++)
            {
                int[] rowValues =GenerateBBSSequence(seedVector[i], _setting.graphOrder,_setting);

                for (int j = 0; j < _setting.graphOrder; j++)
                {
                    subKey[i, j] = rowValues[j];
                }
            }

            return subKey;
        }

    }

}
