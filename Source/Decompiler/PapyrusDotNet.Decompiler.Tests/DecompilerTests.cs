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

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Decompiler.Tests
{
    [TestClass]
    public class DecompilerTests
    {
        [TestMethod]
        public void Decompile_FollowersScript()
        {
            var asm = PapyrusAssemblyDefinition.ReadAssembly(
                @"D:\Git\PapyrusDotNet\Source\Test Scripts\Fallout 4\followersscript.pex");

            var decompiler =
                new PapyrusDecompiler(asm);
            var ctx = decompiler.CreateContext();
            var method =
                asm.Types.First().States.First().Methods.OrderBy(m => m.Body.Instructions.Count).First(m => m.HasBody);
            var result = decompiler.Decompile(ctx, method);
        }
    }
}