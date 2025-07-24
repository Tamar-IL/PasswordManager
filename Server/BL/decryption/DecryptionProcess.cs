using System.Text;
using IBL;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyProject.Common;
using MyProject.Common.Security;

namespace BL.decryption
{
 
    public class DecryptionProcess : IDecryptionProcess
    {
        //private GenerateKeyEncryption generateKeyEncryption;
        private IgenerateKeyDecryption _decryption;
        private readonly MySetting _setting;
        private readonly int[] _keyEncryptionKey;
        private readonly int[,] _initializationMatrix;

        private readonly ILogger<DecryptionProcess> _logger;
        public DecryptionProcess(ISecureKeyProvider keyProvider, IOptions<MySetting> options, ILogger<DecryptionProcess> logger)
        {
            var keyProviderLocal = keyProvider ?? throw new ArgumentNullException(nameof(keyProvider));
            _setting = options?.Value ?? throw new ArgumentNullException(nameof(options));

            // קבל מפתחות מאובטחים
            byte[] masterKey = keyProviderLocal.GetMasterKey();
            int[,] initMatrix = keyProviderLocal.GetInitializationMatrix();

            // המרה ל-int[] לתאימות
            int[] masterKeyInts = new int[masterKey.Length];
            for (int i = 0; i < masterKey.Length; i++)
            {
                masterKeyInts[i] = masterKey[i];
            }

            this._keyEncryptionKey = masterKeyInts;
            this._initializationMatrix = initMatrix;
            _decryption = new generateKeyDecryption(masterKeyInts, options);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        //מפענח הודעה מוצפנת
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

            // מטריצה קודמת עבור CBC (מטריצת אתחול לבלוק הראשון)
            int[,] previousMatrix = _initializationMatrix;

            // פענוח כל בלוק
            for (int i = 0; i < blocksCount; i++)
            {
                // המרת וקטור לבלוק המוצפן למטריצה
                int[,] encryptedMatrix = VectorToMatrix(encryptedBlocks[i]);

                int[,] nextPreviousMatrix = encryptedMatrix;
                // ביצוע XOR עם תת-המפתח
                int[,] modifiedMatrix = CryptographyUtils.MatrixXor(encryptedMatrix, subKeys[i]);

                // ביצוע XOR עם המטריצה הקודמת (CBC)
                int[,] adjacencyMatrix = CryptographyUtils.MatrixXor(modifiedMatrix, previousMatrix);

                // עדכון המטריצה הקודמת לבלוק הבא
                previousMatrix = nextPreviousMatrix;

                // המרת מטריצת הסמיכות לבלוק
                int[] decryptedBlock = AdjacencyMatrixToBlock(adjacencyMatrix);

                decryptedBlocks.Add(decryptedBlock);
            }

            // איחוד כל הבלוקים המפוענחים להודעה אחת
            int[] decryptedMessage = CryptographyUtils.ConcatenateBlocks(decryptedBlocks);
            // שליפת הסיסמה בלי המלח והתחילית 
            int[] parse = GetTextByPrefix(decryptedMessage);
            // המרת מערך ASCII לטקסט והסרת אפסים
            string convertAscii = ConvertAsciiToString(parse);

            return convertAscii;
        }
       
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

        // מחלק הודעה מוצפנת לבלוקים        
        public List<int[]> ParseEncryptedMessage(int[] encryptedMessage, int blocksCount)
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
       
        // ממיר וקטור למטריצה
        public int[,] VectorToMatrix(int[] vector)
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

        // ממיר מטריצת סמיכות לבלוק
        public int[] AdjacencyMatrixToBlock(int[,] adjacencyMatrix)
        {
            // שחזור כל 6 תת-הבלוקים מתוך המעגלים ההמילטוניים
            List<int[]> subBlocks = new List<int[]>();

            for (int subBlockIndex = 0; subBlockIndex < _setting.BlockSize / _setting.subBlockSize; subBlockIndex++)
            {
                int[] subBlock = new int[_setting.subBlockSize];
                List<int> path = CryptographyUtils.CreateHamiltonianCircuit(subBlockIndex);

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
       
        public string ConvertAsciiToString(int[] asciiValues)
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




        //public string Decrypt(int[] encryptedMessage, List<int> vectorOfPositions)
        //{
        //    vectorOfPositions.Reverse();
        //    int[][,] subKeys = _decryption.GenerateSubKeysForDecryption(vectorOfPositions);

        //    int[,] previousMatrix = _initializationMatrix;
        //    int repeat = _setting.numIterationEncrypt;

        //    // התחלה עם הבלוק המוצפן
        //    int[] blockForDecrypt = encryptedMessage;

        //    for (int i = 0; i < repeat; i++)
        //    {
        //        // המרת וקטור למטריצה
        //        int[,] encryptedMatrix = VectorToMatrix(blockForDecrypt);

        //        int[,] nextPreviousMatrix = encryptedMatrix;

        //        // ביצוע XOR עם תת-המפתח
        //        int[,] modifiedMatrix = CryptographyUtils.MatrixXor(encryptedMatrix, subKeys[i]);

        //        // ביצוע XOR עם המטריצה הקודמת
        //        int[,] adjacencyMatrix = CryptographyUtils.MatrixXor(modifiedMatrix, previousMatrix);

        //        // עדכון המטריצה הקודמת
        //        previousMatrix = nextPreviousMatrix;

        //        // המרת מטריצת הסמיכות לבלוק (זה כבר BasicDecrypt!)
        //        int[] decryptedBlock = AdjacencyMatrixToBlock(adjacencyMatrix);

        //        // עדכון הבלוק לאיטרציה הבאה
        //        blockForDecrypt = decryptedBlock;
        //    }

        //    // שליפת הסיסמה בלי המלח
        //    int[] parse = GetTextByPrefix(blockForDecrypt);

        //    // המרת מערך ASCII לטקסט
        //    string convertAscii = ConvertAsciiToString(parse);
        //    return convertAscii;
        //}
        //פונקציה שמטפלת בטקסט  המפוענח להחזיר את הסיסמה בלי המלח
    }
}