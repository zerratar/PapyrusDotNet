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
using PapyrusDotNet.Common.ConsoleExLib;
using PapyrusDotNet.Common.Interfaces;

#endregion

namespace PapyrusDotNet.Common.Utilities
{
    public class ConsoleUiRenderer : IUiRenderer
    {
        private static bool cursorLocked;
        private static int consoleX;
        private static int consoleY;
        private static bool resultVisible;
        private static string lastActionString;

        public void EnsureConsoleCursorPosition()
        {
            if (!cursorLocked)
            {
                consoleX = Console.CursorLeft;
                consoleY = Console.CursorTop;
                cursorLocked = true;
                return;
            }
            Console.CursorLeft = consoleX;
            Console.CursorTop = consoleY;
        }

        public void ReleaseConsoleCursorPosition() => cursorLocked = false;

        public void DrawInterface(string message)
        {
            lastActionString = message;
            ConsoleEx.CursorVisible = false;
            if (resultVisible)
            {
                ConsoleEx.DrawRectangle(BorderStyle.LineSingle, 2, 1, 64, 15, false);
            }
            else
            {
                ConsoleEx.DrawRectangle(BorderStyle.LineSingle, 2, 1, 64, 9, false);
            }
            ConsoleEx.WriteAt(8, 4, "PapyrusDotNet v0.2");
            ConsoleEx.WriteAt(8, 5, "------------------");

            ConsoleEx.WriteAt(8, 7, message.PadRight(32, ' '));
        }

        public void DrawResult(string resultAction)
        {
            ConsoleEx.Clear();
            resultVisible = true;
            DrawInterface(lastActionString);
            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleEx.WriteAt(8, 9, resultAction);
            Console.ResetColor();
        }

        public void DrawHotkeys(params Hotkeys[] hotkeys)
        {
            var pos = 11;
            foreach (var hk in hotkeys)
            {
                ConsoleEx.WriteAt(8, pos, hk.UsageKey);
                pos += 2;
            }
        }

        public void DrawProgressBarWithInfo(int value, int max)
        {
            var text = value + "/" + max;
            ConsoleEx.WriteAt(42, 5, text.PadRight(24));
            ConsoleEx.WriteAt(40, 7, "");
            DrawProgressBar(value, max);
        }

        public void DrawProgressBar(int value, int maxValue)
        {
            EnsureConsoleCursorPosition();
            var originalBg = Console.BackgroundColor;
            var size = 20f;
            Console.Write("  ");
            for (var i = 0; i < size; i++)
            {
                var proc = value/(float) maxValue*size;
                Console.BackgroundColor =
                    i <= proc
                        ? ConsoleColor.Green
                        : ConsoleColor.DarkGray;

                Console.Write(" ");
            }
            Console.WriteLine();
            Console.BackgroundColor = originalBg;
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
    }
}