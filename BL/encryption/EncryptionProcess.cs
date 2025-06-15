using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using BL.decryption;
using IBL;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MyProject.Common;
using System.Security.Cryptography;
using MyProject.Common.Security;
using Microsoft.Extensions.Logging;


namespace BL.encryption
{
    public class EncryptionProcess : IEncryptionProcess
    {
        private static readonly StringBuilder _saltBuilder = new StringBuilder(256);

        private GenerateKeyEncryption generateKeyEncryption;        
        private readonly MySetting _setting;

        //private readonly int[,] _initializationMatrix;
        //private readonly int[] _keyEncryptionKey;
        private readonly ISecureKeyProvider _keyProvider;
        private readonly ILogger<EncryptionProcess> _logger;

        /// <summary>
        /// מצפין הודעת טקסט
        /// </summary>
        /// <param name="clearMessage">הודעה לא-מוצפנת</param>
        /// <returns>הודעה מוצפנת ווקטור מיקומים</returns>
        //public EncryptionProcess(int[] keyEncryptionKey, int[,] initializationMatrix, IOptions<MySetting> options)
        //{
        //    _keyEncryptionKey = keyEncryptionKey;
        //    _initializationMatrix = initializationMatrix;
        //    generateKeyEncryption = new GenerateKeyEncryption(keyEncryptionKey, initializationMatrix, options);
        //    _setting = options.Value;
        //}
        public EncryptionProcess(ISecureKeyProvider keyProvider, IOptions<MySetting> options, ILogger<EncryptionProcess> logger)
        {
            _keyProvider = keyProvider ?? throw new ArgumentNullException(nameof(keyProvider));
            _setting = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // קבל את המפתחות המאובטחים
            byte[] masterKey = _keyProvider.GetMasterKey();
            int[,] initMatrix = _keyProvider.GetInitializationMatrix();

            // המר מ-byte[] ל-int[] לתאימות עם הקוד הקיים
            int[] masterKeyInts = ConvertBytesToInts(masterKey);

            generateKeyEncryption = new GenerateKeyEncryption(masterKeyInts, initMatrix, options);

            _logger.LogInformation("תהליך ההצפנה הופעל עם מפתחות מאובטחים. אורך מפתח: {KeyLength} בתים", masterKey.Length);
        }
        public EncryptionProcess()
        {

        }

        public (int[] EncryptedMessage, List<int> VectorOfPositions) Encrypt(string clearMessage)
        {
            try
            {
                Console.WriteLine($"Input message: '{clearMessage}'");

            clearMessage = AddSaltToMessageEnd(clearMessage);
            Console.WriteLine("clearMessage:" + clearMessage);

            // המרת ההודעה למערך של ערכי ASCII
            int[] messageAsAscii = ConvertMessageToAscii(clearMessage);
            // חישוב מספר הבלוקים
            int messageLength = messageAsAscii.Length;
            int remainder = messageLength % _setting.BlockSize;
            int blocksCount = messageLength / _setting.BlockSize + (remainder > 0 ? 1 : 0);
            // split the message into k' block forming thr set
            // חלוקת ההודעה לבלוקים
            List<int[]> blocks = ParseMessage(messageAsAscii, blocksCount);
            //by algorithm 2 יצירת תת-מפתחות

            var (subKeys, vectorOfPositions) = generateKeyEncryption.GenerateSubKeysForEncryption(blocks);
            // מערך לאחסון הבלוקים המוצפנים
            List<int[]> encryptedBlocks = new List<int[]>();

            // מטריצה קודמת עבור CBC
            //int[,] previousMatrix = _initializationMatrix;
            int[,] previousMatrix = _keyProvider.GetInitializationMatrix();

            // הצפנת כל בלוק
            for (int i = 0; i < blocksCount; i++)
            {
                // חלוקת הבלוק לתת-בלוקים
                List<int[]> subBlocks = ParseBlock(blocks[i]);
                // המרת תת-הבלוקים לגרף עם מעגלים המילטוניים
                int[,] adjacencyMatrix = BlockToAdjacencyMatrix(subBlocks);
                // ביצוע XOR עם המטריצה הקודמת (CBC)
                int[,] modifiedMatrix = CryptographyUtils.MatrixXor(adjacencyMatrix, previousMatrix);
                // ביצוע XOR עם תת-המפתח
                int[,] encryptedMatrix = CryptographyUtils.MatrixXor(modifiedMatrix, subKeys[i]);
                // שמירת המטריצה הנוכחית עבור הבלוק הבא
                previousMatrix = encryptedMatrix;
                // המרת המטריצה המוצפנת לוקטור
                int[] encryptedBlock = MatrixToVector(encryptedMatrix);

                encryptedBlocks.Add(encryptedBlock);
            }
            // איחוד כל הבלוקים המוצפנים לוקטור אחד
            int[] encryptedMessage = CryptographyUtils.ConcatenateBlocks(encryptedBlocks);
            return (encryptedMessage, vectorOfPositions);
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בתהליך ההצפנה");
                throw;
            }
        }
        /// <summary>
        /// מחלק בלוק לתת-בלוקים
        /// </summary>
        public List<int[]> ParseBlock(int[] block)
        {
            List<int[]> subBlocks = new List<int[]>();
            int validLength = Math.Min(block.Length, _setting.BlockSize);

            // חלוקה ל-6 תת-בלוקים באורך 13
            for (int i = 0; i < _setting.BlockSize/_setting.subBlockSize; i++)
            {
                int[] subBlock = new int[_setting.subBlockSize];
                int startIndex = i * _setting.subBlockSize;

                // למקרה של בלוק אחרון שאינו מלא
                if (startIndex < validLength)
                {
                    int copyLength = Math.Min(_setting.subBlockSize, validLength - startIndex);
                    Array.Copy(block, startIndex, subBlock, 0, copyLength);

                    // מילוי עם אפסים אם צריך
                    if (copyLength < _setting.subBlockSize)
                    {
                        for (int j = copyLength; j < _setting.subBlockSize; j++)
                        {
                            subBlock[j] = 0;
                        }
                    }
                }
                else
                {
                    // מילוי תת-בלוק ריק באפסים
                    for (int j = 0; j < _setting.subBlockSize; j++)
                    {
                        subBlock[j] = 0;
                    }
                }

                subBlocks.Add(subBlock);
            }

            return subBlocks;
        }
        /// <summary>
        /// ממיר תת-בלוקים למטריצת סמיכות
        /// </summary>
        public int[,] BlockToAdjacencyMatrix(List<int[]> subBlocks)
        {
            // יצירת מטריצת סמיכות התחלתית מלאה באפסים
            int[,] adjacencyMatrix = new int[_setting.graphOrder, _setting.graphOrder];

            // מעבר על כל תת-בלוק ויצירת מעגל המילטוני
            for (int subBlockIndex = 0; subBlockIndex < subBlocks.Count; subBlockIndex++)
            {
                int[] subBlock = subBlocks[subBlockIndex];

                // יצירת מסלול המילטוני עבור תת-הבלוק
                List<int> path = CryptographyUtils.CreateHamiltonianCircuit(subBlockIndex);

                // הוספת המשקלים      למטריצת הסמיכות
                for (int i = 0; i < path.Count - 1; i++)
                {
                    int value = subBlock[i];
                    int fromVertex = path[i];
                    int toVertex = path[i + 1];

                    adjacencyMatrix[fromVertex, toVertex] = value;
                }

                // סגירת המעגל - חיבור בין הקדקוד האחרון לראשון
                int lastValue = subBlock[path.Count - 1];
                int lastVertex = path[path.Count - 1];
                int firstVertex = path[0];

                adjacencyMatrix[lastVertex, firstVertex] = lastValue;
            }

            return adjacencyMatrix;
        }

        /// <summary>
        /// המרה מ-byte[] ל-int[] לתאימות עם הקוד הקיים
        /// </summary>
        public int[] ConvertBytesToInts(byte[] bytes)
        {
            int[] ints = new int[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                ints[i] = bytes[i];
            }
            return ints;
        }

        /// <summary>
        /// ממיר מטריצה לוקטור
        /// </summary>
        public int[] MatrixToVector(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int[] vector = new int[rows * cols];

            int index = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    vector[index++] = matrix[i, j];
                }
            }

            return vector;
        }

        /// <summary>
        /// מאחד בלוקים לוקטור אחד
        /// </summary>

        //#endregion

        public List<int[]> ParseMessage(int[] message, int blocksCount)
        {
            List<int[]> blocks = new List<int[]>();
            //----------------
            // i think its not good. we need fill the key (length k - like the formula in the top of this page )
            // we don't have to pass on the vP / only take value from VP by randomaly index.
            // fix this loop!!(instead of int position in vectorOfPositions . write  for i =0 to k*13 by the article in algorithm 2 )

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
        public int[] ConvertMessageToAscii(string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            int[] asciiValues = new int[bytes.Length];

            for (int i = 0; i < bytes.Length; i++)
            {
                asciiValues[i] = bytes[i];
            }

            return asciiValues;

        }
        //שמירת אורך הסיסמה לפני הוספת המלח
        public string AddLengthAsPrefix(string input)
        {

            int length = input.Length;

            if (length > 255)
                throw new ArgumentException("Length too long to encode in a single byte.");

            return length + input;
        }

        public  string AddSaltToMessageEnd(string message)
        {         
            message = AddLengthAsPrefix(message);
            int targetLength = _setting.BlockSize;
            string saltChars = _setting.SaltChars;
            _saltBuilder.Clear();
            _saltBuilder.Capacity = Math.Max(_saltBuilder.Capacity, saltChars.Length);

            if (message.Length > targetLength)
                throw new ArgumentException("הסימהה ארוכה מ- ." + _setting.BlockSize + "תווים");           
            int saltLength = targetLength - message.Length;
            //StringBuilder saltBuilder = new StringBuilder(saltLength);
            for (int i = 0; i < saltLength; i++)
            {
                if (saltChars == null)
                    throw new InvalidOperationException("saltChars לא מאותחל");
                int index = RandomNumberGenerator.GetInt32(saltChars.Length);
                char randomChar = saltChars[index];
                _saltBuilder.Append(randomChar);
            }          
            return message + _saltBuilder.ToString();
        }
       

    }

}
