using IBL;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using MyProject.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public class GenerateKeyEncryption: IgenerateKeyEncrryption
    {//every block is length of 78
        // the formula of block's numbers : n=78k +r ----->
        // n-r = 78k ----> (n-r)/78 = 78k/78 ----> k=(n-r)/78 
        // n is the length of plainText , r = remainder of n over 78
        //private static int n;
        //private static int r = n % 78;
        //private static int k = r > 0 ? r : r + 1;     


        public readonly int[] _keyEncryptionKey;
        public readonly int[,] _initializationMatrix;
        public readonly IBBSRandomGenerator _bBSRandomGenerator;
        private readonly MySetting _setting;

        /// <summary>
        /// קונסטרקטור למערכת ההצפנה
        /// </summary>
        /// <param name="keyEncryptionKey">מפתח ראשי</param>
        /// <param name="initializationMatrix">מטריצת אתחול</param>

        public GenerateKeyEncryption(int[] keyEncryptionKey, int[,] initializationMatrix, IOptions<MySetting> options)
        {
            _setting = options?.Value ?? throw new ArgumentNullException(nameof(options));

            if (initializationMatrix.GetLength(0) != _setting.graphOrder || initializationMatrix.GetLength(1) != _setting.graphOrder)
                throw new ArgumentException($"Initialization matrix must be {_setting.graphOrder}x{_setting.graphOrder}");

            _keyEncryptionKey = keyEncryptionKey;
            _initializationMatrix = initializationMatrix;


            //_bBSRandomGenerator = bBSRandomGenerator;
        }

        //#region אלגוריתם 1: יצירת תת-מפתחות בתהליך ההצפנה

        /// <summary>
        /// מייצר תת-מפתחות לתהליך ההצפנה
        /// </summary>
        /// <param name="clearMessage">הודעה לא-מוצפנת</param>
        /// <returns>תת-מפתחות ווקטור מיקומים</returns>
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

                //  selectedChar שלילי, נעשה אותו חיובי (אם זה לא בא מתוך טווח ASCII)
                if (selectedChar < 0)
                {
                    selectedChar = Math.Abs(selectedChar); // או, אם זה לא מספיק,  לאתחל אותו ל-0 במקרה כזה
                }

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

        /// <summary>
        /// מייצר וקטור זרע באורך 13 מתוך ערך התחלתי
        /// </summary>
        public int[] GenerateSeed(int initialValue)
        {
            return BBSRandomGenerator.GenerateBBSSequence(initialValue, _setting.subBlockSize, _setting);
        }
    }
}