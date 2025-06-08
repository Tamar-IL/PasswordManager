using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using IBL;


namespace BL
{
    public class BBSRandomGenerator : IBBSRandomGenerator
    {
        private BigInteger seed;
        private BigInteger modulus;
        public BBSRandomGenerator(BigInteger p, BigInteger q, BigInteger s)
        {
            if(!IsPrime(p) || !IsPrime(q))
                throw new ArgumentException("p and q must be prime numbers");
            modulus = p * q;
            //seed = GenerateSeed(s);
        }
        public BigInteger GenerateSeed( BigInteger s)
        {
           
            // Use RNGCryptoServiceProvider for better security
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] bytes = new byte[modulus.ToByteArray().Length];
                
                do
                {
                    rng.GetBytes(bytes);
                    s = new BigInteger(bytes) % (modulus - 2) + 2; // Ensure 2 <= s < modulus - 1
                } while (BigInteger.GreatestCommonDivisor(s, modulus) != 1);

                return s;
            }
        }
        public async Task<int> Next(int max)
        {
            if (max <= 0)
                throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than 0.");

            // Update the seed using the BBS algorithm
            seed = (seed * seed) % modulus;

            // Return the next random number in the range [0, max)
            return (int)(seed % max);
           
        }
        public  bool IsPrime(BigInteger number)
        {
            if (number < 2) return false;
            //run loop until i*i <= number ,
            //where i*i >= number the remainder will be the number
            //if we check 7 for example  we can see when i = 3 7&9=7 7&10=7 etc. we don't need it.
            for (BigInteger i = 2; i * i <= number; i++)
            {
                if (number % i == 0) return false;
            }
            return true;
        }

      
    }

}
