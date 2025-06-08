using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BL.decryption;
using IBL;


namespace BL.encryption
{
    public class EncryptionProcess : IEncryptionProcess
    {
        private GenerateKeyEncryption generateKeyEncryption;

        private const int BLOCK_SIZE = 15;
        private const int SUB_BLOCK_SIZE = 5;
        private const int GRAPH_ORDER = 5;
        private const int KEY_SIZE = 256;
        //private const int BLOCK_SIZE = 78;
        //private const int SUB_BLOCK_SIZE = 13;
        //private const int GRAPH_ORDER = 13;
        //private const int KEY_SIZE = 256;
        private readonly int[,] _initializationMatrix;
        private readonly int[] _keyEncryptionKey;
        /// <summary>
        /// מצפין הודעת טקסט
        /// </summary>
        /// <param name="clearMessage">הודעה לא-מוצפנת</param>
        /// <returns>הודעה מוצפנת ווקטור מיקומים</returns>
        public EncryptionProcess(int[] keyEncryptionKey, int[,] initializationMatrix)
        {
            _keyEncryptionKey = keyEncryptionKey;
            _initializationMatrix = initializationMatrix;
            generateKeyEncryption = new GenerateKeyEncryption(keyEncryptionKey, initializationMatrix);
        }

        public EncryptionProcess()
        {
        }

        public (int[] EncryptedMessage, List<int> VectorOfPositions) Encrypt(string clearMessage)
        {
            //by algorithm 2 יצירת תת-מפתחות
            var (subKeys, vectorOfPositions) = generateKeyEncryption.GenerateSubKeysForEncryption(clearMessage);

            // המרת ההודעה למערך של ערכי ASCII
            int[] messageAsAscii = ConvertMessageToAscii(clearMessage);

            // חישוב מספר הבלוקים
            int messageLength = messageAsAscii.Length;
            int remainder = messageLength % BLOCK_SIZE;
            int blocksCount = messageLength / BLOCK_SIZE + (remainder > 0 ? 1 : 0);
            // split the message into k' block forming thr set
            // חלוקת ההודעה לבלוקים
            List<int[]> blocks = ParseMessage(messageAsAscii, blocksCount);

            // מערך לאחסון הבלוקים המוצפנים
            List<int[]> encryptedBlocks = new List<int[]>();

            // מטריצה קודמת עבור CBC
            int[,] previousMatrix = _initializationMatrix;

            // הצפנת כל בלוק
            
            for (int i = 0; i < blocksCount; i++)
            {
                // חלוקת הבלוק לתת-בלוקים
                List<int[]> subBlocks = ParseBlock(blocks[i]);
                Console.WriteLine("subBlocks1:");
                foreach (var number in subBlocks)
                {
                    Console.Write(number + " , ");
                }

                // המרת תת-הבלוקים לגרף עם מעגלים המילטוניים
                int[,] adjacencyMatrix = BlockToAdjacencyMatrix(subBlocks);
                Console.WriteLine("adjacencyMatrix1:" );
                foreach (var number in adjacencyMatrix)
                {
                    Console.Write(number + " , ");
                }
                // ביצוע XOR עם המטריצה הקודמת (CBC)
                int[,] modifiedMatrix = MatrixXor(adjacencyMatrix, previousMatrix);
                Console.WriteLine("modifiedMatrix1:");
                foreach (var number in modifiedMatrix)
                {
                    Console.Write(number + " , ");
                }
                // ביצוע XOR עם תת-המפתח
                int[,] encryptedMatrix = MatrixXor(modifiedMatrix, subKeys[i]);
                Console.WriteLine("encryptedMatrix:" );
                foreach (var number in encryptedMatrix)
                {
                    Console.Write(number + " , ");
                }
                // שמירת המטריצה הנוכחית עבור הבלוק הבא
                previousMatrix = encryptedMatrix;

                // המרת המטריצה המוצפנת לוקטור
                int[] encryptedBlock = MatrixToVector(encryptedMatrix);

                encryptedBlocks.Add(encryptedBlock);
            }
            Console.WriteLine("encryptedBlock:" );
            foreach (var number in encryptedBlocks)
            {
                Console.Write(number+",");
            }
            // איחוד כל הבלוקים המוצפנים לוקטור אחד
            int[] encryptedMessage = ConcatenateBlocks(encryptedBlocks);
            Console.Write("encryptedMessage:" + encryptedMessage);
            foreach (var number in encryptedMessage)
            {
                Console.WriteLine(number + ",");
            }
            return (encryptedMessage, vectorOfPositions);
        }

        /// <summary>
        /// מחלק בלוק לתת-בלוקים
        /// </summary>
        private List<int[]> ParseBlock(int[] block)
        {
            List<int[]> subBlocks = new List<int[]>();
            int validLength = Math.Min(block.Length, BLOCK_SIZE);

            // חלוקה ל-6 תת-בלוקים באורך 13
            for (int i = 0; i < 3; i++)
            {
                int[] subBlock = new int[SUB_BLOCK_SIZE];
                int startIndex = i * SUB_BLOCK_SIZE;

                // למקרה של בלוק אחרון שאינו מלא
                if (startIndex < validLength)
                {
                    int copyLength = Math.Min(SUB_BLOCK_SIZE, validLength - startIndex);
                    Array.Copy(block, startIndex, subBlock, 0, copyLength);

                    // מילוי עם אפסים אם צריך
                    if (copyLength < SUB_BLOCK_SIZE)
                    {
                        for (int j = copyLength; j < SUB_BLOCK_SIZE; j++)
                        {
                            subBlock[j] = 0;
                        }
                    }
                }
                else
                {
                    // מילוי תת-בלוק ריק באפסים
                    for (int j = 0; j < SUB_BLOCK_SIZE; j++)
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
        private int[,] BlockToAdjacencyMatrix(List<int[]> subBlocks)
        {
            // יצירת מטריצת סמיכות התחלתית מלאה באפסים
            int[,] adjacencyMatrix = new int[GRAPH_ORDER, GRAPH_ORDER];

            // מעבר על כל תת-בלוק ויצירת מעגל המילטוני
            for (int subBlockIndex = 0; subBlockIndex < subBlocks.Count; subBlockIndex++)
            {
                int[] subBlock = subBlocks[subBlockIndex];

                // יצירת מסלול המילטוני עבור תת-הבלוק
                List<int> path = CreateHamiltonianCircuit(subBlockIndex);

                // הוספת המשקלים      למטריצת הסמיכות
                for (int i = 0; i < path.Count - 1; i++)
                {
                    int value = subBlock[i];
                    int fromVertex = path[i];
                    int toVertex = path[i + 1];

                    adjacencyMatrix[fromVertex, toVertex] = value;
                    adjacencyMatrix[toVertex, fromVertex] = value; // גרף לא מכוון
                }

                // סגירת המעגל - חיבור בין הקדקוד האחרון לראשון
                int lastValue = subBlock[path.Count - 1];
                int lastVertex = path[path.Count - 1];
                int firstVertex = path[0];

                adjacencyMatrix[lastVertex, firstVertex] = lastValue;
                adjacencyMatrix[firstVertex, lastVertex] = lastValue; // גרף לא מכוון
            }

            return adjacencyMatrix;
        }

        /// <summary>
        /// יוצר מעגל המילטוני עבור אינדקס תת-בלוק
        /// </summary>
        private List<int> CreateHamiltonianCircuit(int subBlockIndex)
        {
            // כאן נייצר 6 מעגלים המילטוניים זרים בגרף מסדר 13
            // כל מעגל מייצג דרך שונה לעבור על כל הקדקודים פעם אחת וחזרה לקדקוד ההתחלתי

            // בפועל, לכל subBlockIndex יש מעגל קבוע מראש
            switch (subBlockIndex)
            {
                //case 0:
                //    return new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
                //case 1:
                //    return new List<int> { 0, 2, 4, 6, 8, 10, 12, 11, 9, 7, 5, 3, 1 };
                //case 2:
                //    return new List<int> { 0, 3, 6, 9, 12, 8, 4, 1, 5, 10, 7, 2, 11 };
                //case 3:
                //    return new List<int> { 0, 4, 8, 12, 7, 3, 10, 6, 2, 9, 5, 1, 11 };
                //case 4:
                //    return new List<int> { 0, 5, 10, 4, 9, 3, 8, 2, 7, 1, 6, 11, 12 };
                //case 5:
                //    return new List<int> { 0, 6, 1, 7, 12, 5, 11, 4, 10, 3, 9, 2, 8 };
                //default:
                case 0:
                    return new List<int> { 0, 1, 2, 3, 4  };
                case 1:
                    return new List<int> { 0, 2, 4, 3, 1 };
                case 2:
                    return new List<int> { 0, 3, 1, 2, 4 };
                case 3:
                    return new List<int> { 0, 4, 3, 1, 2 };
                case 4:
                    return new List<int> { 0, 3, 4, 2, 1 };
                case 5:
                    return new List<int> { 0, 2, 1, 3, 4 };
                default:
                    throw new ArgumentException("Invalid sub-block index");
            }
        }

        /// <summary>
        /// מבצע פעולת XOR בין שתי מטריצות
        /// </summary>
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

        /// <summary>
        /// ממיר מטריצה לוקטור
        /// </summary>
        private int[] MatrixToVector(int[,] matrix)
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

        //#endregion

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
    }
}
