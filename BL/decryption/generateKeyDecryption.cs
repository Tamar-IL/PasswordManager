using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using IBL;

namespace BL.decryption
{
    public class generateKeyDecryption : IgenerateKeyDecryption
    {
        // the formula of block's numbers : n=78k +r ----->
        // n-r = 78k ----> (n-r)/78 = 78k/78 ----> k=(n-r)/78 
        // n is the length of plainText , r = remainder of n over 78
        private static int n=156;
        private static int r = n % 78;
        private static int k = r > 0 ? n/78+1 : n/78 ;
        //private const int BLOCK_SIZE = 78;
        private const int SUB_BLOCK_SIZE = 13;
        private const int GRAPH_ORDER = 13;
        //private const int KEY_SIZE = 256;
        private readonly int[] _keyEncryptionKey;
        private int[,] initMatrix;

        public generateKeyDecryption(int[] keyEncryptionKey, int[,] initMatrix)
        {
            _keyEncryptionKey = keyEncryptionKey;
            this.initMatrix = initMatrix;

        }

        /// <summary>
        /// מייצר תת-מפתחות לתהליך הפענוח
        /// </summary>
        /// <param name="vectorOfPositions">וקטור המיקומים</param>
        /// <returns>מערך של תת-מפתחות</returns>
        //הפונק אמורה לקבל גם את המסטר KEK לא שתהיה בתור תכונה..
        public int[][,] GenerateSubKeysForDecryption(List<int> vectorOfPositions)
        {
            int blocksCount = vectorOfPositions.Count;

            // יצירת מפתח מלא מתוך וקטור המיקומים והמפתח הראשי
            int[] key = GenerateKey(_keyEncryptionKey, vectorOfPositions);

            // חלוקת המפתח לווקטורים
            List<int[]> seedVectors = ParseKey(key, blocksCount);

            // יצירת תת-מפתחות מהווקטורים
            int[][,] subKeys = new int[blocksCount][,];
            // again not blockCount. k'.
            for (int i = 0; i < blocksCount; i++)
            {
                subKeys[i] = GenerateSubKey(seedVectors[i]);
            }
            for (int i = 0; i < subKeys.Length; i++)
            {
                for (int j = 0; j < subKeys.Length; j++)
                {
                    for (int k = 0; k < subKeys.Length; k++)
                    {
                        Console.Write(subKeys[i][j,k]);
                    }
                    Console.WriteLine("---");
                }
                Console.WriteLine("---");

            }
            return subKeys;
        }

        /// <summary>
        /// מייצר מפתח מלא מתוך וקטור המיקומים והמפתח הראשי
        /// </summary>
        private int[] GenerateKey(int[] keyEncryptionKey, List<int> vectorOfPositions)
        {
            Console.WriteLine("--------------------------------------------");
            for (int i =0; i < vectorOfPositions.Count(); i++)
            {
                Console.WriteLine(vectorOfPositions[i]);
            }
            Console.WriteLine("--------------------------------------------");

            //int keyLength = SUB_BLOCK_SIZE * vectorOfPositions.Count;
            int keyLength = SUB_BLOCK_SIZE * vectorOfPositions.Count;
            int[] key = new int[keyLength];
            int index = 0;
            foreach (int position in vectorOfPositions)  // לא במעגל!
            {
                int seed = keyEncryptionKey[position % keyEncryptionKey.Length];
                int[] values = GenerateBBSSequence(seed, SUB_BLOCK_SIZE);
                Array.Copy(values, 0, key, index, SUB_BLOCK_SIZE);
                index += SUB_BLOCK_SIZE;
            }
            //foreach (int position in vectorOfPositions)
            //{
            //    // קבלת ערך מהמפתח הראשי
            //    int seed = keyEncryptionKey[position % keyEncryptionKey.Length];

            //    // יצירת רצף מספרים
            //    int[] values = GenerateBBSSequence(seed, SUB_BLOCK_SIZE);

            //    // הוספה למפתח
            //    Array.Copy(values, 0, key, index, SUB_BLOCK_SIZE);
            //    index += SUB_BLOCK_SIZE;
            //}

            return key;
        }

        /// <summary>
        /// מחלק את המפתח לווקטורים
        /// </summary>
        /// ----------------
        // block count its k' , again, like i write in the top  this page. 
        // we have to divide the key into k' vectors .
        private List<int[]> ParseKey(int[] key, int blocksCount)
        {
            List<int[]> seedVectors = new List<int[]>();

            for (int i = 0; i < blocksCount; i++)
            {
                //int[] seedVector = new int[k];

                int[] seedVector = new int[SUB_BLOCK_SIZE];
                Array.Copy(key, i * SUB_BLOCK_SIZE, seedVector, 0, SUB_BLOCK_SIZE);
                //Array.Copy(key, i * k, seedVector, 0, k);
                seedVectors.Add(seedVector);
            }
            return seedVectors;
        }

        /// <summary>
        /// מייצר תת-מפתח מווקטור זרע
        /// </summary>
        private int[,] GenerateSubKey(int[] seedVector)
        {
            int[,] subKey = new int[GRAPH_ORDER, GRAPH_ORDER];

            for (int i = 0; i < GRAPH_ORDER; i++)
            {
                int[] rowValues = GenerateBBSSequence(seedVector[i], GRAPH_ORDER);

                for (int j = 0; j < GRAPH_ORDER; j++)
                {
                    subKey[i, j] = rowValues[j];
                }
            }
           
            return subKey;
        }
        private int[] GenerateBBSSequence(int seed, int length)
        {
            // מספרים ראשוניים גדולים עבור BBS
            BigInteger p = BigInteger.Parse("98799821657648109045695379286138768173");
            BigInteger q = BigInteger.Parse("65147279015126562838267191403654601389");
            BigInteger m = p * q;

            int[] sequence = new int[length];

            // הבטחה שהזרע הוא חיובי
            BigInteger x = new BigInteger(Math.Abs(seed));
            x = (x * x) % m;  // x₀ = seed² mod m

            for (int i = 0; i < length; i++)
            {
                x = (x * x) % m;  // xₙ₊₁ = xₙ² mod m
                sequence[i] = (int)(x % 256);  // מיפוי לטווח 0-255
            }

            return sequence;
        }

//#endregion


    }
}
