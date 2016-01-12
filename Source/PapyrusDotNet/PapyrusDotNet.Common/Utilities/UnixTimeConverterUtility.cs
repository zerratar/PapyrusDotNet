//     This file is part of PapyrusDotNet.
// 
//     PapyrusDotNet is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     PapyrusDotNet is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with PapyrusDotNet.  If not, see <http://www.gnu.org/licenses/>.
//  
//     Copyright 2016, Karl Patrik Johansson, zerratar@gmail.com

#region

using System;
using PapyrusDotNet.Common.Interfaces;

#endregion

namespace PapyrusDotNet.Common.Utilities
{
    public class UnixTimeConverterUtility : IUtility
    {
        /// <summary>
        ///     Converts the specified datetime into a Unix Timestamp.
        /// </summary>
        /// <param name="value">The datetime</param>
        /// <returns></returns>
        public static int Convert(DateTime value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            var span = value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();

            //return the total seconds (which is a UNIX timestamp)
            return (int) span.TotalSeconds;
        }

        /// <summary>
        ///     Converts the unix timestamp into a DateTime.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static DateTime Convert(int value)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime().AddSeconds(value);
        }
    }
}