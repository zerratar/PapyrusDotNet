using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PapyrusDotNet.System
{
    using PapyrusDotNet.Core;

    public class Random
    {
        public static double Range(float min, float max)
        {
            return Utility.RandomFloat(min, max);
        }

        public static int RangeInt(int min, int max)
        {
            return Utility.RandomInt(min, max);
        }
    }
}
