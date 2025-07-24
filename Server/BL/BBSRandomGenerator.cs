using MyProject.Common;
using Org.BouncyCastle.Math;
using System.Security.Cryptography;
using Org.BouncyCastle.Security; 

namespace BL
{
    public static  class BBSRandomGenerator   
    {
        public static BigInteger GenerateBlumPrime(int bitLength, SecureRandom random)
        {
            
            while (true)
            {
                // יצירת מספר ראשוני באורך הביטים הנתון
                BigInteger prime = new BigInteger(bitLength, 100, random); // 100 הוא רמת הוודאות (iterations)

                // בדיקה האם הוא מקיים P = 3 mod 4
                if (prime.Mod(BigInteger.ValueOf(4)).Equals(BigInteger.ValueOf(3)))
                {
                    return prime;
                }
            }
        }

        public static Tuple<BigInteger, BigInteger> GenerateBlumPrimesPair(int bitLength)
        {

            SecureRandom random = new SecureRandom(); 

            BigInteger p = GenerateBlumPrime(bitLength, random);
            BigInteger q;

            do
            {
                q = GenerateBlumPrime(bitLength, random);
            } while (q.Equals(p)); // לוודא ש-P ו-Q שונים
            
            return new Tuple<BigInteger, BigInteger>(p, q);
        }
        //יצירת וקטור מזרע 
        public static int[] GenerateBBSSequence(int seed, int length,MySetting _setting)
        {
            BigInteger P = new BigInteger(_setting.P);
            BigInteger Q =  new BigInteger(_setting.Q);
            var M = P.Multiply( Q);

            int[] sequence = new int[length];

            // הבטחה שהזרע הוא חיובי
            BigInteger x = new BigInteger(Math.Abs(seed).ToString());
            x = x.Multiply(x).Mod(M);  // x₀ = seed² mod m
            int i = 0;
            while (i < length)
            {
                x = x.Multiply(x).Mod(M);
                int k = x.Mod(BigInteger.ValueOf(_setting.keySize)).IntValue;

                if (k >= 2)
                {
                    sequence[i] = k;
                    i++;
                }
            }

            return sequence;
        }
        //
        public static int[,] GenerateSubKey(int[] seedVector, MySetting _setting)
        {
            int[,] subKey = new int[_setting.graphOrder, _setting.graphOrder];

            for (int i = 0; i < _setting.graphOrder; i++)
            {
                //יצצירת  וקטור על ידי המחולל  כך שהזרע הוא המערך שקיבלנו
                int[] rowValues = GenerateBBSSequence(seedVector[i], _setting.graphOrder,_setting);
                //הכנסת הערכים למפתח ממה שהמחולל יצר
                for (int j = 0; j < _setting.graphOrder; j++)
                {
                    subKey[i, j] = rowValues[j];
                }
            }

            return subKey;
        }

    }

}
