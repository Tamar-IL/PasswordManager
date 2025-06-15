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
                subKeys[i] = BBSRandomGenerator.GenerateSubKey(seedVectors[i],_setting);
            }
            return subKeys;
        }

        /// <summary>
        /// מייצר מפתח מלא מתוך וקטור המיקומים והמפתח הראשי
        /// </summary>
        public int[] GenerateKey(int[] keyEncryptionKey, List<int> vectorOfPositions)
        {
            int keyLength = _setting.subBlockSize * vectorOfPositions.Count;
            int[] key = new int[keyLength];
            int index = 0;
            foreach (int position in vectorOfPositions) 
            {
                int keyIndex = Math.Abs(position) % _setting.keySize;
                int seed = keyEncryptionKey[keyIndex];

                int[] values = BBSRandomGenerator.GenerateBBSSequence(seed, _setting.subBlockSize, _setting);
                Array.Copy(values, 0, key, index, _setting.subBlockSize);
                index += _setting.subBlockSize;

            }

            return key;

        }
           
        /// <summary>
        /// מחלק את המפתח לווקטורים
        /// </summary>
        /// ----------------
        // block count its k' , again, like i write in the top at this page. 
        // we have to divide the key into k' vectors .
        public List<int[]> ParseKey(int[] key, int blocksCount)
        {
            List<int[]> seedVectors = new List<int[]>();

            for (int i = 0; i < blocksCount; i++)
            {
                int[] seedVector = new int[_setting.subBlockSize];
                Array.Copy(key, i * _setting.subBlockSize, seedVector, 0, _setting.subBlockSize);
                seedVectors.Add(seedVector);
            }
            return seedVectors;
        }

    }
}
