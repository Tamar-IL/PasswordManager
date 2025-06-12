using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBL;
using Microsoft.Extensions.Options;
using MyProject.Common;

namespace BL.decryption
{

    //the block is length of 78
    // the formula of block's numbers : n=78k +r ----->
    // n-r = 78k ----> (n-r)/78 = 78k/78 ----> k=(n-r)/78 
    // n is the length of plainText , r = remainder of n over 78
    //private static int n;
    //private static int r = n % 78;
    //private static int k = r > 0 ? r : r + 1;  
    public class DecryptionProcess : IDecryptionProcess
    {
        //private GenerateKeyEncryption generateKeyEncryption;
        private IgenerateKeyDecryption _decryption;
        private readonly MySetting _setting;
        private readonly int[] _keyEncryptionKey;
        private readonly int[,] _initializationMatrix;

        //public DecryptionProcess(GenerateKeyEncryption generateKeyEncryption, generateKeyDecryption decryption, int[] keyEncryptionKey, int[,] initializationMatrix)
        public DecryptionProcess(int[] keyEncryptionKey, int[,] initializationMatrix, IOptions<MySetting> options)
        {
            //this.generateKeyEncryption = generateKeyEncryption;
            //this.decryption = decryption ?? throw new ArgumentNullException(nameof(decryption));  // אם decryption הוא null, נשלח חריגה
            this._keyEncryptionKey = keyEncryptionKey;
            this._initializationMatrix = initializationMatrix;
            _decryption = new generateKeyDecryption(keyEncryptionKey, initializationMatrix, options);
            _setting = options.Value;

        }
        /// <summary>
        /// מפענח הודעה מוצפנת
        /// </summary>
        /// <param name="encryptedMessage">הודעה מוצפנת</param>
        /// <param name="vectorOfPositions">וקטור מיקומים</param>
        /// <returns>הודעה מפוענחת</returns>
        /// 
        public string Decrypt(int[] encryptedMessage, List<int> vectorOfPositions)
        {
            // יצירת תת-מפתחות
            int[][,] subKeys = _decryption.GenerateSubKeysForDecryption(vectorOfPositions);

            // חישוב מספר הבלוקים
            // למה מספר הבלוקים הוא כמספר האורך של וקטור המיקומים???...
            int blocksCount = vectorOfPositions.Count;

            // חלוקת ההודעה המוצפנת לבלוקים
            List<int[]> encryptedBlocks = ParseEncryptedMessage(encryptedMessage, blocksCount);

            // מערך לאחסון הבלוקים המפוענחים
            List<int[]> decryptedBlocks = new List<int[]>();
            //-----------------
            // למה שזה לא יהיה המטריצה האחרונה עם הראשונה?????...
            //-----------------
            // מטריצה קודמת עבור CBC (מטריצת אתחול לבלוק הראשון)
            int[,] previousMatrix = _initializationMatrix;

            // פענוח כל בלוק
            for (int i = 0; i < blocksCount; i++)
            {
                // המרת וקטור לבלוק המוצפן למטריצה
                int[,] encryptedMatrix = VectorToMatrix(encryptedBlocks[i]);
                int[,] nextPreviousMatrix = encryptedMatrix;
                // ביצוע XOR עם תת-המפתח
                int[,] modifiedMatrix = MatrixXor(encryptedMatrix, subKeys[i]);

                // ביצוע XOR עם המטריצה הקודמת (CBC)
                int[,] adjacencyMatrix = MatrixXor(modifiedMatrix, previousMatrix);

                // עדכון המטריצה הקודמת לבלוק הבא
                previousMatrix = nextPreviousMatrix;

                // המרת מטריצת הסמיכות לבלוק
                int[] decryptedBlock = AdjacencyMatrixToBlock(adjacencyMatrix);

                decryptedBlocks.Add(decryptedBlock);
            }

            // איחוד כל הבלוקים המפוענחים להודעה אחת
            int[] decryptedMessage = ConcatenateBlocks(decryptedBlocks);
            // שליפת הסיסמה בלי המלח והתחילית 
            int[] parse = GetTextByPrefix(decryptedMessage);
            // המרת מערך ASCII לטקסט והסרת אפסים
            string convertAscii = ConvertAsciiToString(parse);

            return convertAscii;
        }
        //פונקציה שמטפלת בטקסט  המפוענח להחזיר את הסיסמה בלי המלח
        public int[] GetTextByPrefix(int[] asciiArray)
        {
            int num = 1;
            if (asciiArray == null || asciiArray.Length == 0)
                return Array.Empty<int>();
            int count = (char)asciiArray[0];
            int digit = count - '0';
            if (digit < _setting.minPass)
            {
                 digit = (asciiArray[0] - '0') * 10 + (asciiArray[1] - '0');
                 num = 2;
            }

            // הגבלת האורך שלא יחרוג מהאורך של המערך המקורי
            count = Math.Min(digit, asciiArray.Length - num);
            return asciiArray.Skip(num).Take(count).ToArray();
        }
        /// <summary>
        /// מחלק הודעה מוצפנת לבלוקים
        /// </summary>
        private List<int[]> ParseEncryptedMessage(int[] encryptedMessage, int blocksCount)
        {
            List<int[]> blocks = new List<int[]>();
            int blockLength = _setting.graphOrder * _setting.graphOrder; // 13^2 = 169

            for (int i = 0; i < blocksCount; i++)
            {
                int startIndex = i * blockLength;
                if (startIndex >= encryptedMessage.Length)
                    break;

                int[] block = new int[blockLength];
                int copyLength = Math.Min(blockLength, encryptedMessage.Length - startIndex);
                Array.Copy(encryptedMessage, startIndex, block, 0, copyLength);

                blocks.Add(block);
            }

            return blocks;
        }
        /// <summary>
        /// ממיר וקטור למטריצה
        /// </summary>
        private int[,] VectorToMatrix(int[] vector)
        {
            int side = (int)Math.Sqrt(vector.Length); // צריך להיות 13
            int[,] matrix = new int[side, side];

            int index = 0;
            for (int i = 0; i < side; i++)
            {
                for (int j = 0; j < side; j++)
                {
                    matrix[i, j] = vector[index++];
                }
            }

            return matrix;
        }
        /// <summary>
        /// ממיר מטריצת סמיכות לבלוק
        /// </summary>
        private int[] AdjacencyMatrixToBlock(int[,] adjacencyMatrix)
        {
            // שחזור כל 6 תת-הבלוקים מתוך המעגלים ההמילטוניים
            List<int[]> subBlocks = new List<int[]>();

            for (int subBlockIndex = 0; subBlockIndex < _setting.BlockSize / _setting.subBlockSize; subBlockIndex++)
            {
                int[] subBlock = new int[_setting.subBlockSize];
                List<int> path = CreateHamiltonianCircuit(subBlockIndex);

                // הוצאת הערכים מהמטריצה לפי המסלול
                for (int i = 0; i < path.Count - 1; i++)
                {
                    int fromVertex = path[i];
                    int toVertex = path[i + 1];
                    subBlock[i] = adjacencyMatrix[fromVertex, toVertex];
                }
                // הערך האחרון - מהקדקוד האחרון לראשון
                int lastVertex = path[path.Count - 1];
                int firstVertex = path[0];
                subBlock[path.Count - 1] = adjacencyMatrix[lastVertex, firstVertex];

                subBlocks.Add(subBlock);
            }
            // איחוד כל תת-הבלוקים לבלוק אחד
            int[] block = new int[_setting.BlockSize];
            int index = 0;

            foreach (int[] subBlock in subBlocks)
            {
                Array.Copy(subBlock, 0, block, index, subBlock.Length);
                index += subBlock.Length;
            }
            return block;
        }
        private int[,] MatrixXor(int[,] matrix1, int[,] matrix2)
        {
            int rows = matrix1.GetLength(0);
            int cols = matrix1.GetLength(1);
            int[,] result = new int[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result[i, j] = matrix1[i, j] ^ matrix2[i, j];
                }
            }

            return result;


        }
        /// </summary>
        private string ConvertAsciiToString(int[] asciiValues)
        {
            // הסרת אפסים מסופיים
            int validLength = asciiValues.Length;
            while (validLength > 0 && asciiValues[validLength - 1] == 0)
            {
                validLength--;
            }
            byte[] bytes = new byte[validLength];

            for (int i = 0; i < validLength; i++)
            {
                bytes[i] = (byte)asciiValues[i];
            }
            return Encoding.ASCII.GetString(bytes);
        }
        private List<int> CreateHamiltonianCircuit(int subBlockIndex)
        {
            // כאן נייצר 6 מעגלים המילטוניים זרים בגרף מסדר 13
            // כל מעגל מייצג דרך שונה לעבור על כל הקדקודים פעם אחת וחזרה לקדקוד ההתחלתי
            // בפועל, לכל subBlockIndex יש מעגל קבוע מראש
            switch (subBlockIndex)
            {
                case 0:
                    return new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
                case 1:
                    return new List<int> { 0, 2, 4, 6, 8, 10, 12, 11, 9, 7, 5, 3, 1 };
                case 2:
                    return new List<int> { 0, 3, 6, 9, 12, 8, 4, 1, 5, 10, 7, 2, 11 };
                case 3:
                    return new List<int> { 0, 4, 8, 12, 7, 3, 10, 6, 2, 9, 5, 1, 11 };
                case 4:
                    return new List<int> { 0, 5, 10, 4, 9, 3, 8, 2, 7, 1, 6, 11, 12 };
                case 5:
                    return new List<int> { 0, 6, 1, 7, 12, 5, 11, 4, 10, 3, 9, 2, 8 };
                default:
                    throw new ArgumentException("Invalid sub-block index");
            }
        }
        //פונקציה ס=זו נמצאת גם בהצפנה וגם בפענוח כמו פונקציות המרה לאסקי או מאסקי וכו
        private int[] ConcatenateBlocks(List<int[]> blocks)
        {
            int totalLength = blocks.Sum(block => block.Length);
            int[] result = new int[totalLength];

            int index = 0;
            foreach (int[] block in blocks)
            {
                Array.Copy(block, 0, result, index, block.Length);
                index += block.Length;
            }

            return result;

        }
    }
}