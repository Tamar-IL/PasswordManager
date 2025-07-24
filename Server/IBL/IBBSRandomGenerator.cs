using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MyProject.Common;


namespace IBL
{
    public interface IBBSRandomGenerator
    {
        int[,] GenerateSubKey(int[] seedVector, MySetting _setting);
        int[] GenerateBBSSequence(int seed, int length, MySetting _setting);
       


    }
}
