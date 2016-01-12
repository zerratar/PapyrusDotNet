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

using System.Collections.Generic;
using System.IO;
using PapyrusDotNet.Common.Interfaces;

#endregion

namespace PapyrusDotNet.Converters.Papyrus2CSharp
{
    public class MultiCSharpOutput : IAssemblyOutput
    {
        private readonly IEnumerable<CSharpOutput> outputs;

        public MultiCSharpOutput(IEnumerable<CSharpOutput> outputs)
        {
            this.outputs = outputs;
        }

        public void Save(string output)
        {
            foreach (var o in outputs)
            {
                o.Save(output);
            }
        }
    }

    public class CSharpOutput : IAssemblyOutput
    {
        private readonly string outputFileContent;
        private readonly string outputFileName;

        public CSharpOutput(string outputFileName, string outputFileContent)
        {
            this.outputFileName = outputFileName;
            this.outputFileContent = outputFileContent;
        }

        public void Save(string output)
        {
            var filePath =
                Path.Combine(output, outputFileName.Replace(":", "_"));
            File.WriteAllText(filePath, outputFileContent);
        }
    }
}