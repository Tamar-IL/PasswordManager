using System.Text;
using IBL;
using Microsoft.Extensions.Options;
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
        private readonly ISecureKeyProvider _keyProvider;
        private readonly ILogger<EncryptionProcess> _logger;

        public EncryptionProcess(ISecureKeyProvider keyProvider, IOptions<MySetting> options, ILogger<EncryptionProcess> logger)
        {
            /// בדיקה שהם לא NULL
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

        public (int[] EncryptedMessage, List<int> VectorOfPositions) Encrypt(string clearMessage)
        {
            try
            {
                //הוספת מלח ואורך לסיסמא
                clearMessage = AddSaltToMessageEnd(clearMessage);
                // המרת ההודעה למערך של ערכי ASCII
                int[] messageAsAscii = ConvertMessageToAscii(clearMessage);
                int repeatCount = _setting.numIterationEncrypt; // מס פעמים לחזור
                int[] test = new int[messageAsAscii.Length * repeatCount];

                // יצירת מערך חדש שמכיל את המערך המקורי 3 פעמים
                int[] repeatedArray = new int[messageAsAscii.Length * repeatCount];

                for (int i = 0; i < repeatCount; i++)
                {
                    Array.Copy(messageAsAscii, 0, repeatedArray, i * messageAsAscii.Length, messageAsAscii.Length);
                }
                // חישוב מספר הבלוקים
                int messageLength = repeatedArray.Length;
                int remainder = messageLength % _setting.BlockSize;
                int blocksCount = messageLength / _setting.BlockSize + (remainder > 0 ? 1 : 0);
                // split the message into k' block forming thr set
                // חלוקת  לבלוקים
                List<int[]> blocks = ParseMessage(repeatedArray, blocksCount);

                //by algorithm 2 יצירת תת-מפתחות

                var (subKeys, vectorOfPositions) = generateKeyEncryption.GenerateSubKeysForEncryption(blocks);
                Console.Write(subKeys);
                // מערך לאחסון הבלוקים המוצפנים
                List<int[]> encryptedBlocks = new List<int[]>();

                // מטריצה קודמת עבור CBC
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
                    //הוספה לבלוקים המוצפנים
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
       

        // מחלק בלוק לתת-בלוקים
        public List<int[]> ParseBlock(int[] block)
        {
            List<int[]> subBlocks = new List<int[]>();
            int validLength = Math.Min(block.Length, _setting.BlockSize);

            // חלוקה ל-6 תת-בלוקים באורך 13
            for (int i = 0; i < _setting.BlockSize/_setting.subBlockSize; i++)
            {
                int[] subBlock = new int[_setting.subBlockSize];
                int startIndex = i * _setting.subBlockSize;

                /// למקרה של בלוק אחרון שאינו מלא
                /// אם הוא מלא startIndex יהיה 0 ואז יועתק כל תת הבלוק..
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
       
        // ממיר תת-בלוקים למטריצת סמיכו       
        public int[,] BlockToAdjacencyMatrix(List<int[]> subBlocks)
        {
            // יצירת מטריצת סמיכות התחלתית עם אפסים
            int[,] adjacencyMatrix = new int[_setting.graphOrder, _setting.graphOrder];

            // מעבר על כל תת בלוק ויצירת מעגל המילטוני
            for (int subBlockIndex = 0; subBlockIndex < subBlocks.Count; subBlockIndex++)
            {
                int[] subBlock = subBlocks[subBlockIndex];

                // יצירת מסלול המילטוני עבור תת הבלוק
                List<int> path = CryptographyUtils.CreateHamiltonianCircuit(subBlockIndex);

                // הוספת המשקלים  למטריצת הסמיכות
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
        
        // המרה מ-byte[] ל-int[] לתאימות עם הקוד הקיים        
        public int[] ConvertBytesToInts(byte[] bytes)
        {
            int[] ints = new int[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                ints[i] = bytes[i];
            }
            return ints;
        }

        // ממיר מטריצה לוקטור       
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

        public List<int[]> ParseMessage(int[] message, int blocksCount)
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
            //ניקוי
            _saltBuilder.Clear();
            // מספיק מקום מראש
            _saltBuilder.Capacity = Math.Max(_saltBuilder.Capacity, saltChars.Length);

            if (message.Length > targetLength)
                throw new ArgumentException("הסיסמה ארוכה מ- ." + _setting.BlockSize + "תווים"); 
            
            int saltLength = targetLength - message.Length;
            //StringBuilder saltBuilder = new StringBuilder(saltLength);
             if (saltChars == null)
                    throw new InvalidOperationException("saltChars לא מאותחל");

            for (int i = 0; i < saltLength; i++)
            {               
                int index = RandomNumberGenerator.GetInt32(saltChars.Length);
                char randomChar = saltChars[index];
                _saltBuilder.Append(randomChar);
            }          
            return message + _saltBuilder.ToString();
        }



        //public (int[] EncryptedMessage, List<int> VectorOfPositions) Encrypt(string clearMessage)
        //{
        //    try
        //    {
        //        //הוספת מלח ואורך לסיסמא
        //        clearMessage = AddSaltToMessageEnd(clearMessage);
        //        // המרת ההודעה למערך של ערכי ASCII
        //        int[] messageAsAscii = ConvertMessageToAscii(clearMessage);
        //        int repeatCount = _setting.numIterationEncrypt; // מס פעמים לחזור

        //        List<int> vectorOfPositions = new List<int>();
        //        int[] blockForEncrypt = messageAsAscii;
        //        // טעינת מטריצה אתחול עבור הבלוק הראשון  
        //        int[,] previousMatrix = _keyProvider.GetInitializationMatrix();
        //        int[,] adjacencyMatrix;
        //        int[,] key;
        //        int position;
        //        int[] encryptedBlock = new int[_setting.BlockSize];
        //        // הצפנת כל בלוק
        //        for (int i = 0; i < repeatCount; i++)
        //        {                 
        //            (adjacencyMatrix, position,key)= BasicEncrypt(blockForEncrypt);
        //            Console.WriteLine("keyencrypt" + i + " " );
        //            for(int j = 0; j< 13; j++)
        //            {
        //                for (int k = 0; k <13; k++)
        //                {
        //                    Console.Write(key[j,k]+" , ");

        //                }
        //            }
        //            vectorOfPositions.Add(position);
        //            // ביצוע XOR עם המטריצה הקודמת (CBC)
        //            int[,] modifiedMatrix = CryptographyUtils.MatrixXor(adjacencyMatrix, previousMatrix);
        //            // ביצוע XOR עם תת-המפתח
        //            int[,] encryptedMatrix = CryptographyUtils.MatrixXor(modifiedMatrix, key);
        //            // שמירת המטריצה הנוכחית עבור הבלוק הבא
        //            previousMatrix = encryptedMatrix;
        //            // המרת המטריצה המוצפנת לוקטור
        //            encryptedBlock = MatrixToVector(encryptedMatrix);
        //            blockForEncrypt = encryptedBlock;
        //        }

        //        return (encryptedBlock, vectorOfPositions);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "שגיאה בתהליך ההצפנה");
        //        throw;
        //    }
        //}
        //public (int[,],int vp,int[,] key) BasicEncrypt(int[] block)
        //{
        //    // חלוקת הבלוק לתת-בלוקים
        //    List<int[]> subBlocks = ParseBlock(block);
        //    // המרת תת-הבלוקים לגרף עם מעגלים המילטוניים
        //    int[,] adjacencyMatrix = BlockToAdjacencyMatrix(subBlocks);

        //    var (Key, Position) = generateKeyEncryption.GenerateSubKeysForEncryption(block);

        //    return (adjacencyMatrix, Position, Key);
        //}
    }

}
