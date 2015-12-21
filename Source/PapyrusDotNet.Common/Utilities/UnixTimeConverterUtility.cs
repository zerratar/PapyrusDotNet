using System;
using PapyrusDotNet.Common.Interfaces;

namespace PapyrusDotNet.Common.Utilities
{
    public class UnixTimeConverterUtility : IUtility
    {
        /// <summary>
        /// Converts the specified datetime into a Unix Timestamp.
        /// </summary>
        /// <param name="value">The datetime</param>
        /// <returns></returns>
        public static int Convert(DateTime value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            var span = value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();

            //return the total seconds (which is a UNIX timestamp)
            return (int)span.TotalSeconds;
        }

        /// <summary>
        /// Converts the unix timestamp into a DateTime.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static DateTime Convert(int value)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime().AddSeconds(value);
        }
    }
}