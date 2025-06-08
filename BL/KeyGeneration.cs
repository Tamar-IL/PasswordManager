using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using IBL;


 namespace BL
{
    //לבדוק איך עובד לוקים והמפתחות ולסדר את ה מחולל מפתחות פסאודו אקראיים 
    public class KeyGeneration : IkeyGeneration
    {
        private BigInteger p;
        private BigInteger q;
        private BigInteger seed;
        private BBSRandomGenerator bbs;
        private BigInteger modulus;
        List<int> VP = new List<int>();
        List<int> KEK = new List<int>(0);
        int[][] setSubKeys = new int[13][];
        List<int[][]> subKeys = new List<int[][]>();
        public KeyGeneration(BigInteger p, BigInteger q, BigInteger n)
        {
            bbs = new BBSRandomGenerator(p, q, n);
            this.p = p;
            this.q = q;
            modulus = p * q;
        }
        public async Task<List<int>> ConvertMessage(String message, int n)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("Message cannot be null or empty", nameof(message));

            List<int> cma = new List<int>();
            foreach (Char c in message)
            {
                cma.Add((int)c);
            }

            return cma;
        }

        //חלוקת ההודעה ל k בלוקים

        public async Task<List<List<int>>> parseMessage(List<int> cma, int k)
        {
            List<List<int>> blockset = new List<List<int>>();
            int lengthCma = cma.Count();
            int sizeBlock = lengthCma / k, j = 0, i = 0;
            int remainder = lengthCma % k;
            for (; i < k; i++)
            {
                List<int> ki = new List<int>();
                for (; j < sizeBlock; j++)
                {
                    ki.Add(cma[j]);
                }
                blockset.Add(ki);

            }
            // בשביל השארית אם הטקסט לא מתחלק בדיוק
            if (remainder != 0)
            {
                List<int> ka = new List<int>();
                for (; j < remainder; j++)
                {
                    ka.Add(cma[j]);
                }
                blockset.Add(ka);

            }
            return blockset;
        }
        //בחירה רנדומלית של תו מבלוק

        public async Task<int> getCharFromBlock(List<int> Blocki)
        {
            //הגרלת מיקום בתוך בלוק מסוים		
            Random randIndex = new Random();
            int index = randIndex.Next(1, Blocki.Count());
            int i = Blocki[index];
            return i;
        }

        public void addToVP(int c)
        {
            VP.Add(c);
        }

        //מחזיר את הערך במיקום P במפתח KEK כאשר P מייצג את האסקי
        //של התו הרלונטי.נניח האסקי 65 אז ניגש למיקום 65 במערך הKEK
        public async Task<int> getNbrFromKEK(List<int> KEK, int p)
        {
            return KEK[p];
        }

        //יצירת וקטור SI בגודל 13 מ N שקיבלנו קודם ע"י BBS - מחולל מספרים פסאודו אקראיים
        public async Task<int[]> generateSeed(BigInteger n)
        {
            int[] si = new int[13];

            for (int i = 0; i < 13; i++)
            {
                int a = (int)bbs.GenerateSeed(n);
                si[i] = a;
            }
            return si;
        }
        public async Task<int[][]> generateSubKey(int[] si)
        {
            for (int i = 0; i < 13; i++)
            {
                int[] a = await generateSeed(si[i]);
                setSubKeys[i] = a;
            }
            return setSubKeys;
        }

        //מזין את SKi באמצעות מפתחות המשנה setSubKeys
        public async Task putSubKey(int[][] subKey)
        {
            subKeys.Add(subKey);
        }
        public async Task<(List<int[][]>, List<int>)> generateKey(string message, int k, List<int> KEK)
        {
            List<int> cma = await ConvertMessage(message, message.Length);
            List<List<int>> blockSet = await parseMessage(cma, k);
            foreach (List<int> block in blockSet)
            {
                int charAscii = await getCharFromBlock(block);
                addToVP(charAscii);
                BigInteger N = await getNbrFromKEK(KEK, charAscii);
                int[] seed = await generateSeed(N);
                int[][] subkey = await generateSubKey(seed);
                putSubKey(subkey);
            }
            return (subKeys, VP);
        }
    }
}
