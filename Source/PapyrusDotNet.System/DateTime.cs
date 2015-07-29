namespace PapyrusDotNet.System
{
    using PapyrusDotNet.Core;

    public class DateTime
    {
        public static double Now
        {
            get
            {                
                return Utility.GetCurrentRealTime();
            }
        }

        public static double Min
        {
            get
            {
                return 0;
            }
        }

        public static double Max
        {
            get
            {
                return 99999999999;
            }
        }
    }
}
