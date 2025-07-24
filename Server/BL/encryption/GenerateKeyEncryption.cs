using IBL;
using Microsoft.Extensions.Options;
using MyProject.Common;
using System.Security.Cryptography;


namespace BL
{
    public class GenerateKeyEncryption : IgenerateKeyEncrryption
    {
        public readonly int[] _keyEncryptionKey;
        public readonly int[,] _initializationMatrix;
        public readonly IBBSRandomGenerator _bBSRandomGenerator;
        private readonly MySetting _setting;

        public GenerateKeyEncryption(int[] keyEncryptionKey, int[,] initializationMatrix, IOptions<MySetting> options)
        {
            _setting = options?.Value ?? throw new ArgumentNullException(nameof(options));

            if (initializationMatrix.GetLength(0) != _setting.graphOrder || initializationMatrix.GetLength(1) != _setting.graphOrder)
                throw new ArgumentException($"Initialization matrix must be {_setting.graphOrder}x{_setting.graphOrder}");

            _keyEncryptionKey = keyEncryptionKey;
            _initializationMatrix = initializationMatrix;
        }

        public (int[][,] SubKeys, List<int> VectorOfPositions) GenerateSubKeysForEncryption(List<int[]> blocks)
        {
            int blocksCount = blocks.Count();
            int[][,] subKeys = new int[blocks.Count()][,];
            List<int> vectorOfPositions = new List<int>();

            // עבור כל בלוק יוצרים תת-מפתח

            for (int i = 0; i < blocksCount; i++)
            {
                // בחירה אקראית של תו מהבלוק
                int randomIndex = RandomNumberGenerator.GetInt32(blocks[i].Length);
                int selectedChar = blocks[i][randomIndex];

                ////  selectedChar שלילי, נעשה אותו חיובי (אם זה לא בא מתוך טווח ASCII)
                //if (selectedChar < 0)
                //{
                //    selectedChar = Math.Abs(selectedChar); // או, אם זה לא מספיק,  לאתחל אותו ל-0 במקרה כזה
                //}

                // הוספה לוקטור המיקומים
                vectorOfPositions.Add(selectedChar);


                int index = selectedChar % _keyEncryptionKey.Length;  // מודול 128

                // אם האינדקס לא בטווח, תפסול את החישוב
                if (index < 0 || index >= _keyEncryptionKey.Length)
                {
                    throw new ArgumentOutOfRangeException($"האינדקס {index} חורג מהמגבלה של המערך.");
                }

                // קבלת ערך מהמפתח הראשי
                int n = _keyEncryptionKey[index];
                // יצירת וקטור זרע באורך 13
                int[] seedVector = GenerateSeed(n);
                // יצירת תת-מפתח
                subKeys[i] = BBSRandomGenerator.GenerateSubKey(seedVector, _setting);
            }

            return (subKeys, vectorOfPositions);
        }
       
        /// מייצר וקטור זרע באורך 13 מתוך ערך התחלתי
        public int[] GenerateSeed(int initialValue)
        {
            return BBSRandomGenerator.GenerateBBSSequence(initialValue, _setting.subBlockSize, _setting);
        }





        //public (int[,] Key, int Position) GenerateSubKeysForEncryption(int[] blocks)
        //{
        //    int blocksCount = blocks.Count();
        //    int[,] Key;

        //    // בחירה אקראית של תו מהבלוק
        //    int randomIndex = RandomNumberGenerator.GetInt32(blocks.Length);
        //    int Position = blocks[randomIndex];

        //    int index = Position % _keyEncryptionKey.Length;  // מודול 128

        //    // אם האינדקס לא בטווח, תפסול את החישוב
        //    if (index < 0 || index >= _keyEncryptionKey.Length)
        //    {
        //        throw new ArgumentOutOfRangeException($"האינדקס {index} חורג מהמגבלה של המערך.");
        //    }

        //    // קבלת ערך מהמפתח הראשי
        //    int n = _keyEncryptionKey[index];
        //    // יצירת וקטור זרע באורך 13
        //    int[] seedVector = GenerateSeed(n);
        //    // יצירת תת-מפתח
        //    Key = BBSRandomGenerator.GenerateSubKey(seedVector, _setting);
        //    return (Key, Position);
        //}
       
    }
}