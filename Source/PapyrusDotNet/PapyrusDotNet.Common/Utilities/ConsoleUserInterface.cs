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

using System;
using PapyrusDotNet.Common.Interfaces;

namespace PapyrusDotNet.Common.Utilities
{
    public class ConsoleUserInterface : IUserInterface
    {
        public void Clear()
        {
            Console.Clear();
        }

        public void DrawInterface(string message)
        {
            Console.WriteLine(message);
        }

        public void DrawResult(string message)
        {
            Console.WriteLine(message);
        }

        public void DrawHotkeys(params Hotkeys[] hotkeys)
        {
        }

        public void DrawProgressBarWithInfo(int value, int max)
        {
        }

        public void DrawProgressBar(int value, int maxValue)
        {
        }

        public void DrawHelp()
        {
            Console.WriteLine(
                        "  Usage: PapyrusDotNet.exe <input> <output> [option] [<target papyrus version (-fo4 | -skyrim)>] [<compiler settings (-strict|-easy)>]");
            Console.WriteLine("Options:");
            Console.WriteLine(
                "  \t-papyrus :: [Default] Converts a .NET .dll into .pex files. Each class will be a separate .pex file.");
            Console.WriteLine("  \t* <input> :: file (.dll)");
            Console.WriteLine("  \t* <output> :: folder");
            Console.WriteLine("  \t* <target version> :: [Fallout 4 is default] -fo4 or -skyrim");
            Console.WriteLine(
                "  \t* <compiler options> :: [Strict is default] -strict or -easy determines how the compiler will react on features known to not work in papyrus. -strict will throw a build exception while -easy may let it slide and just remove the usage but may cause problems with the final script.");
            Console.WriteLine(
                "  \t-clr :: Converts a .pex or folder containg .pex files into a .NET library usable when modding.");
            Console.WriteLine("  \t* <input> :: .pex file or folder");
            Console.WriteLine("  \t* <output> :: folder (File will be named PapyrusDotNet.Core.dll)");
            Console.WriteLine(
                "  \t-x :: Skips the waiting keyinput the process is completed and closes PapyrusDotNet automatically.");
        }

        public void DrawError(string error)
        {
            Console.WriteLine(error);
        }
    }
}