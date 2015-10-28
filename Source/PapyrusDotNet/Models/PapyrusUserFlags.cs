/*
    This file is part of PapyrusDotNet.

    PapyrusDotNet is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    PapyrusDotNet is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with PapyrusDotNet.  If not, see <http://www.gnu.org/licenses/>.
	
	Copyright 2015, Karl Patrik Johansson, zerratar@gmail.com
 */

namespace PapyrusDotNet.Models
{
    using System;
    using System.Collections.Generic;

    public class PapyrusUserFlags
    {
        public Dictionary<string, int> Flags { get; set; }

        public PapyrusUserFlags()
        {
            Flags = new Dictionary<string, int>();
            Flags.Add("conditional", 1);
            Flags.Add("hidden", 0);
        }

        public override string ToString()
        {
            string output = "";
            output += ".userFlagsRef" + Environment.NewLine;
            foreach (var flag in Flags)
            {
                output += "\t.flag " + flag.Key + " " + flag.Value + Environment.NewLine;
            }
            output += ".endUserFlagsRef" + Environment.NewLine;
            return output;
        }
    }
}