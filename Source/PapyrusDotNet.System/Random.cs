using PapyrusDotNet.Core;

namespace PapyrusDotNet.System
{
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