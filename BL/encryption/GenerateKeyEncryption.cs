using IBL;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

        private const int BLOCK_SIZE = 78;
        private const int SUB_BLOCK_SIZE = 13;
        private const int GRAPH_ORDER = 13;
        private const int KEY_SIZE = 256;

        public readonly int[] _keyEncryptionKey;
        public readonly int[,] _initializationMatrix;

        /// <summary>
        /// קונסטרקטור למערכת ההצפנה
        /// </summary>
        /// <param name="keyEncryptionKey">מפתח ראשי</param>
        /// <param name="initializationMatrix">מטריצת אתחול</param>

        public GenerateKeyEncryption(int[] keyEncryptionKey, int[,] initializationMatrix)
        {
            //if (keyEncryptionKey.Length != KEY_SIZE)
            //    throw new ArgumentException($"Master key must be {KEY_SIZE} bytes long");

            if (initializationMatrix.GetLength(0) != GRAPH_ORDER || initializationMatrix.GetLength(1) != GRAPH_ORDER)
                throw new ArgumentException($"Initialization matrix must be {GRAPH_ORDER}x{GRAPH_ORDER}");

            _keyEncryptionKey = keyEncryptionKey;
            _initializationMatrix = initializationMatrix;
        }

        //#region אלגוריתם 1: יצירת תת-מפתחות בתהליך ההצפנה

        /// <summary>
        /// מייצר תת-מפתחות לתהליך ההצפנה
        /// </summary>
        /// <param name="clearMessage">הודעה לא-מוצפנת</param>
        /// <returns>תת-מפתחות ווקטור מיקומים</returns>
        public (int[][,] SubKeys, List<int> VectorOfPositions) GenerateSubKeysForEncryption(string clearMessage)
        {
            // המרת ההודעה למערך של ערכי ASCII
            int[] messageAsAscii = ConvertMessageToAscii(clearMessage);
            Console.WriteLine("message as ascii ", string.Join(", ",messageAsAscii)); 
            // חישוב מספר הבלוקים
            int messageLength = messageAsAscii.Length;
          
            int remainder = messageLength % BLOCK_SIZE;

            int blocksCount = messageLength / BLOCK_SIZE + (remainder > 0 ? 1 : 0);

            // חלוקת ההודעה לבלוקים
            List<int[]> blocks = ParseMessage(messageAsAscii, blocksCount);
            Console.WriteLine("blocks ",string.Join(", ",blocks));
            int[][,] subKeys = new int[blocksCount][,];
            List<int> vectorOfPositions = new List<int>();

            // עבור כל בלוק יוצרים תת-מפתח
            for (int i = 0; i < blocksCount; i++)
            {
                // בחירה אקראית של תו מהבלוק
                int randomIndex = new Random().Next(blocks[i].Length);
                int selectedChar = blocks[i][randomIndex];

                // אם selectedChar שלילי, נעשה אותו חיובי (אם זה לא בא מתוך טווח ASCII)
                if (selectedChar < 0)
                {
                    selectedChar = Math.Abs(selectedChar); // או, אם זה לא מספיק, תוכל לאתחל אותו ל-0 במקרה כזה
                }

                // הוספה לוקטור המיקומים
                vectorOfPositions.Add(selectedChar);
                
                // קבלת מספר מהמפתח הראשי
                //למה לחחלק ל256? כי אם קיבלנו בסלקטצאר מספר יותר גדול מ256 
                //אז אם נבצע חלוקה ב256 יכול להיות שנקבל מספר גדול מ256
                //לעומת זאת עם נבצע שארית 256 המספר הגדול ביותר שנוכל לקבל זה 255
                //KEKואז ודאי לא תהיה חריגה מגודל המערך 

                int index = selectedChar % KEY_SIZE;  // מודול 256

                // אם האינדקס לא בטווח, תפסול את החישוב
                if (index < 0 || index >= _keyEncryptionKey.Length)
                {
                    throw new ArgumentOutOfRangeException($"האינדקס {index} חורג מהמגבלה של המערך.");
                }

                // קבלת ערך מהמפתח הראשי
                int n = _keyEncryptionKey[index];

                // יצירת וקטור זרע באורך 13
                int[] seedVector = GenerateSeed(n);
                Console.WriteLine("seed vector: ",string.Join(", ",seedVector));
                // יצירת תת-מפתח
                subKeys[i] = GenerateSubKey(seedVector);
                Console.WriteLine("subkey [",i,"]",string.Join(", ", subKeys[i]));
            }
            for (int i = 0; i < subKeys.Length; i++)
            {
                for (int j = 0; j < subKeys.Length; j++)
                {
                    for (int k = 0; k < subKeys.Length; k++)
                    {
                        Console.Write(subKeys[i][j, k]);
                    }
                    Console.WriteLine("---");

                }
                Console.WriteLine("---");

            }
            return (subKeys, vectorOfPositions);
        }

        /// <summary>
        /// מייצר וקטור זרע באורך 13 מתוך ערך התחלתי
        /// </summary>
        private int[] GenerateSeed(int initialValue)
        {
            return GenerateBBSSequence(initialValue, SUB_BLOCK_SIZE);

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
            int[,] subKey = new int[GRAPH_ORDER, GRAPH_ORDER];

            for (int i = 0; i < GRAPH_ORDER; i++)
            {
                int[] rowValues = GenerateBBSSequence(seedVector[i], GRAPH_ORDER);

                for (int j = 0; j < GRAPH_ORDER; j++)
                {
                    subKey[i, j] = rowValues[j];
                }
            }
            Console.WriteLine("subkey length: ",subKey.Length);
            return subKey;
        }

        private List<int[]> ParseMessage(int[] message, int blocksCount)
        {
            List<int[]> blocks = new List<int[]>();
            //----------------
            // i think its not good. we need fill the key (length k - like the formula in the top of this page )
            // we don't have to pass on the vP / only take value from VP by randomaly index.
            // fix this loop!!(instead of int position in vectorOfPositions . write  for i =0 to k*13 by the article in algorithm 2 )

            for (int i = 0; i < blocksCount; i++)
            {
                int startIndex = i * BLOCK_SIZE;
                int[] block = new int[BLOCK_SIZE];

                // העתקת הנתונים לבלוק (או מילוי באפסים אם בסוף)
                int copyLength = Math.Min(BLOCK_SIZE, message.Length - startIndex);
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