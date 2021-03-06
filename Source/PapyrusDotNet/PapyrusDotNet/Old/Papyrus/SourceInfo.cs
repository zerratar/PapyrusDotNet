﻿//     This file is part of PapyrusDotNet.
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
using PapyrusDotNet.Common.Utilities;

#endregion

namespace PapyrusDotNet.Old.Papyrus
{
    public class SourceInfo
    {
        public SourceInfo()
        {
            Source = "PapyrusDotNet-Generated.psc";
            ModifyTime = UnixTimeConverterUtility.Convert(DateTime.Now);
            CompileTime = ModifyTime;
            User = Environment.UserName;
            Computer = Environment.MachineName;
        }

        public string Source { get; set; }
        public int ModifyTime { get; set; }
        public int CompileTime { get; set; }
        public string User { get; set; }
        public string Computer { get; set; }

        public override string ToString()
        {
            var output = "";
            output += ".info" + Environment.NewLine;
            output += "\t.source \"" + Source + "\"" + Environment.NewLine;
            output += "\t.modifyTime " + ModifyTime + Environment.NewLine;
            output += "\t.compileTime " + CompileTime + Environment.NewLine;
            output += "\t.user \"" + User + "\"" + Environment.NewLine;
            output += "\t.computer \"" + Computer + "\"" + Environment.NewLine;
            output += ".endInfo" + Environment.NewLine;
            return output;
        }
    }
}