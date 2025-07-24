using IBL;
using Microsoft.Extensions.Options;
using MyProject.Common;

namespace BL.decryption
{
    public class generateKeyDecryption : IgenerateKeyDecryption
    {
       
        private readonly int[] _keyEncryptionKey;

        private readonly MySetting _setting;

        public generateKeyDecryption(int[] keyEncryptionKey, IOptions<MySetting> options)
        {
            _keyEncryptionKey = keyEncryptionKey;
            _setting = options.Value;

        }

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
                //שליחת הזרע - VP  וממנו יצירת מפתח
                subKeys[i] = BBSRandomGenerator.GenerateSubKey(seedVectors[i],_setting);
            }
            return subKeys;
        }

        // מייצר מפתח מלא מתוך וקטור המיקומים והמפתח הראשי     
        public int[] GenerateKey(int[] keyEncryptionKey, List<int> vectorOfPositions)
        {
            int keyLength = _setting.subBlockSize * vectorOfPositions.Count;
            int[] key = new int[keyLength];
            int index = 0;
            foreach (int position in vectorOfPositions) 
            {
                int keyIndex = Math.Abs(position) % _keyEncryptionKey.Length;
                int seed = keyEncryptionKey[keyIndex];

                int[] values = BBSRandomGenerator.GenerateBBSSequence(seed, _setting.subBlockSize, _setting);
                Array.Copy(values, 0, key, index, _setting.subBlockSize);
                index += _setting.subBlockSize;

            }

            return key;

        }
           

        /// מחלק את המפתח לווקטורים
        /// ----------------
       
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
