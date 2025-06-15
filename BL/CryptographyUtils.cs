using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public static class CryptographyUtils
    {
        public static int[,] MatrixXor(int[,] matrix1, int[,] matrix2)
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
        public static List<int> CreateHamiltonianCircuit(int subBlockIndex)
        {
            // כאן נייצר 6 מעגלים המילטוניים זרים בגרף מסדר 13
            // כל מעגל מייצג דרך שונה לעבור על כל הקדקודים פעם אחת וחזרה לקדקוד ההתחלתי
            // בפועל, לכל subBlockIndex יש מעגל קבוע מראש
            switch (subBlockIndex)
            {
                case 0:
                    return new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
                case 1:
                    return new List<int> { 0, 2, 4, 6, 8, 10, 12, 1, 3, 5, 7, 9, 11 };
                case 2:
                    return new List<int> { 0, 3, 6, 9, 12, 2, 5, 8, 11, 1, 4, 7, 10 };
                case 3:
                    return new List<int> { 0, 4, 8, 12, 3, 7, 11, 2, 6, 10, 1, 5, 9 };
                case 4:
                    return new List<int> { 0, 5, 10, 2, 7, 12, 4, 9, 1, 6, 11, 3, 8 };
                case 5:
                    return new List<int> { 0, 6, 12, 5, 11, 4, 10, 3, 9, 2, 8, 1, 7 };
                default:
                    throw new ArgumentException("Invalid sub-block index");
            }
        }
        public static int[] ConcatenateBlocks(List<int[]> blocks)
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
