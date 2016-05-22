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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Mono.Cecil;
using PapyrusDotNet.Common.Extensions;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.Converters.Clr2Papyrus;
using PapyrusDotNet.Converters.Clr2Papyrus.Enums;
using PapyrusDotNet.Converters.Clr2Papyrus.Interfaces;
using PapyrusDotNet.Converters.Papyrus2Clr;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var noUi = args.Contains("-noui");
            var conf = new ContainerConfiguration(noUi);
            var app = new PapyrusDotNetApp(args,
                    conf.Resolve<IUserInterface>(),
                    conf.Resolve<IClrInstructionProcessor>(),
                    conf.Resolve<INameConventionResolver>()
                );
            return app.Run();
        }
        // ld is to load from stack and assign its value to either function, variable or return

        // st codes are to store to the stack
    }
}