using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IBL;
using Microsoft.Extensions.Options;
using MyProject.Common;

namespace BL.decryption
{
    public class generateKeyDecryption : IgenerateKeyDecryption
    {
        // the formula of block's numbers : n=78k +r ----->
        // n-r = 78k ----> (n-r)/78 = 78k/78 ----> k=(n-r)/78 
        // n is the length of plainText , r = remainder of n over 78
        // n=156;
        // r = n % 78;
        // k = r > 0 ? n/78+1 : n/78 ;
       
        private readonly int[] _keyEncryptionKey;
        private int[,] initMatrix;
        private readonly MySetting _setting;

        public generateKeyDecryption(int[] keyEncryptionKey, int[,] initMatrix, IOptions<MySetting> options)
        {
            _keyEncryptionKey = keyEncryptionKey;
            this.initMatrix = initMatrix;
            _setting = options.Value;

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
            //for (int i = 0; i < subKeys.Length; i++)
            //{
            //    for (int j = 0; j < subKeys.Length; j++)
            //    {
            //        for (int k = 0; k < subKeys.Length; k++)
            //        {
            //            Console.Write(subKeys[i][j,k]);
            //        }
            //        Console.WriteLine("---");
            //    }
            //    Console.WriteLine("---");

            //}
            return subKeys;
        }

        /// <summary>
        /// מייצר מפתח מלא מתוך וקטור המיקומים והמפתח הראשי
        /// </summary>
        public int[] GenerateKey(int[] keyEncryptionKey, List<int> vectorOfPositions)
        {
            Console.WriteLine("--------------------------------------------");
            for (int i =0; i < vectorOfPositions.Count(); i++)
            {
                Console.WriteLine(vectorOfPositions[i]);
            }
            Console.WriteLine("--------------------------------------------");

            //int keyLength = SUB_BLOCK_SIZE * vectorOfPositions.Count;
            int keyLength = _setting.subBlockSize * vectorOfPositions.Count;
            int[] key = new int[keyLength];
            int index = 0;
            foreach (int position in vectorOfPositions)  // לא במעגל!
            {
                //int seed = keyEncryptionKey[position % keyEncryptionKey.Length];

                //int[] values = GenerateBBSSequence(seed, SUB_BLOCK_SIZE);
                //Array.Copy(values, 0, key, index, SUB_BLOCK_SIZE);
                ////index += SUB_BLOCK_SIZE;
                /////-------------------שינוי
                //int keyIndex = position % _setting.keySize;
                //if (keyIndex < 0 || keyIndex >= keyEncryptionKey.Length)
                //{
                //    throw new ArgumentOutOfRangeException($"האינדקס {keyIndex} חורג מהמגבלה של המערך.");
                //}
                //int seed = keyEncryptionKey[keyIndex];
                //int[] values = GenerateBBSSequence(seed, _setting.subBlockSize);
                //Array.Copy(values, 0, key, index, _setting.subBlockSize);
                //index += _setting.subBlockSize;
                //----------שינוי א---------
                // וודא שהאינדקס תקין
                int keyIndex = Math.Abs(position) % _setting.keySize;
                int seed = keyEncryptionKey[keyIndex];

                int[] values = GenerateBBSSequence(seed, _setting.subBlockSize);
                Array.Copy(values, 0, key, index, _setting.subBlockSize);
                index += _setting.subBlockSize;

            }

            return key;

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

        

        /// <summary>
        /// מחלק את המפתח לווקטורים
        /// </summary>
        /// ----------------
        // block count its k' , again, like i write in the top  this page. 
        // we have to divide the key into k' vectors .
        public List<int[]> ParseKey(int[] key, int blocksCount)
        {
            List<int[]> seedVectors = new List<int[]>();

            for (int i = 0; i < blocksCount; i++)
            {
                //int[] seedVector = new int[k];

                int[] seedVector = new int[_setting.subBlockSize];
                Array.Copy(key, i * _setting.subBlockSize, seedVector, 0, _setting.subBlockSize);
                //Array.Copy(key, i * k, seedVector, 0, k);
                seedVectors.Add(seedVector);
            }
            return seedVectors;
        }

        /// <summary>
        /// מייצר תת-מפתח מווקטור זרע
        /// </summary>
        public int[,] GenerateSubKey(int[] seedVector)
        {
            int[,] subKey = new int[_setting.graphOrder, _setting.graphOrder];

            for (int i = 0; i < _setting.graphOrder; i++)
            {
                int[] rowValues = GenerateBBSSequence(seedVector[i], _setting.graphOrder);

                for (int j = 0; j < _setting.graphOrder; j++)
                {
                    subKey[i, j] = rowValues[j];
                }
            }
           
            return subKey;
        }
        public int[] GenerateBBSSequence(int seed, int length)
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
