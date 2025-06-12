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
    public class GenerateKeyEncryption
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
            // המרת ההודעה למערך של ערכי ASCII
            //int[] messageAsAscii = ConvertMessageToAscii(clearMessage);
          
            // חישוב מספר הבלוקים
            //int messageLength = messageAsAscii.Length;

            //int remainder = messageLength % _setting.BlockSize;

            //int blocksCount = messageLength / _setting.BlockSize + (remainder > 0 ? 1 : 0);
            int blocksCount = blocks.Count();

            // חלוקת ההודעה לבלוקים
            //List<int[]> blocks = ParseMessage(messageAsAscii, blocksCount);
            //Console.WriteLine("blocks ", string.Join(", ", blocks));
            int[][,] subKeys = new int[blocks.Count()][,];
            List<int> vectorOfPositions = new List<int>();
            // יצירת זרע מבוסס על התוכן

            //Random random = new Random(blocks.GetHashCode());

            // עבור כל בלוק יוצרים תת-מפתח

            for (int i = 0; i < blocksCount; i++)
            {
                // בחירה אקראית של תו מהבלוק
                //int randomIndex = random.Next();
                int randomIndex = RandomNumberGenerator.GetInt32(blocks[i].Length);
                int selectedChar = blocks[i][randomIndex];

                // אם selectedChar שלילי, נעשה אותו חיובי (אם זה לא בא מתוך טווח ASCII)
                if (selectedChar < 0)
                {
                    selectedChar = Math.Abs(selectedChar); // או, אם זה לא מספיק, תוכל לאתחל אותו ל-0 במקרה כזה
                }

                // הוספה לוקטור המיקומים
                vectorOfPositions.Add(selectedChar);

                
                int index = selectedChar % _setting.keySize;  // מודול 256

                // אם האינדקס לא בטווח, תפסול את החישוב
                if (index < 0 || index >= _keyEncryptionKey.Length)
                {
                    throw new ArgumentOutOfRangeException($"האינדקס {index} חורג מהמגבלה של המערך.");
                }

                // קבלת ערך מהמפתח הראשי
                int n = _keyEncryptionKey[index];

                // יצירת וקטור זרע באורך 13
                int[] seedVector = GenerateSeed(n);
                Console.WriteLine("seed vector: ", string.Join(", ", seedVector));
                // יצירת תת-מפתח
                subKeys[i] = GenerateSubKey(seedVector);
                Console.WriteLine("subkey [", i, "]", string.Join(", ", subKeys[i]));
            }
          
            return (subKeys, vectorOfPositions);
        }

        /// <summary>
        /// מייצר וקטור זרע באורך 13 מתוך ערך התחלתי
        /// </summary>
        private int[] GenerateSeed(int initialValue)
        {
            //BigInteger p, q, s = new BigInteger;
            //return _bBSRandomGenerator.GenerateSeed(p,q,s);
            return GenerateBBSSequence(initialValue, _setting.subBlockSize);

        }
        private int[] ConvertMessageToAscii(string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            int[] asciiValues = new int[bytes.Length];

            for (int i = 0; i < bytes.Length; i++)
            {
                asciiValues[i] = bytes[i];
            }

            return asciiValues;
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
        private int[,] GenerateSubKey(int[] seedVector)
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
            Console.WriteLine("subkey length: ", subKey.Length);
            return subKey;
        }

        private List<int[]> ParseMessage(int[] message, int blocksCount)
        {
            List<int[]> blocks = new List<int[]>();

            for (int i = 0; i < blocksCount; i++)
            {
                int startIndex = i * _setting.BlockSize;
                int[] block = new int[_setting.BlockSize];

                // העתקת הנתונים לבלוק (או מילוי באפסים אם בסוף)
                int copyLength = Math.Min(_setting.BlockSize, message.Length - startIndex);
                if (copyLength > 0)
                {
                    Array.Copy(message, startIndex, block, 0, copyLength);
                }

                blocks.Add(block);
            }

            return blocks;
        }
    }
}