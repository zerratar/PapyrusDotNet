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
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace PapyrusDotNet.Common.ConsoleExLib
{
    public enum BorderStyle
    {
        None,
        Text,
        LineSingle,
        LineDouble
    }

    public enum ConsoleForeground
    {
        Black,
        Navy,
        DarkGreen,
        Aquamarine,
        Maroon,
        Purple,
        Olive,
        LightGray,
        DarkGray,
        Blue,
        Green,
        Cyan,
        Red,
        Magenta,
        Yellow,
        White
    }

    public enum ConsoleBackground
    {
        Aquamarine = 0x30,
        Black = 0,
        Blue = 0x90,
        Cyan = 0xb0,
        DarkGray = 0x80,
        DarkGreen = 0x20,
        Green = 160,
        LightGray = 0x70,
        Magenta = 0xd0,
        Maroon = 0x40,
        Navy = 0x10,
        Olive = 0x60,
        Purple = 80,
        Red = 0xc0,
        White = 240,
        Yellow = 0xe0
    }

    [Flags]
    public enum InputModeFlags
    {
        ENABLE_ECHO_INPUT = 4,
        ENABLE_LINE_INPUT = 2,
        ENABLE_MOUSE_INPUT = 0x10,
        ENABLE_PROCESSED_INPUT = 1,
        ENABLE_WINDOW_INPUT = 8
    }

    [Flags]
    public enum OutputModeFlags
    {
        ENABLE_PROCESSED_OUTPUT = 1,
        ENABLE_WRAP_AT_EOL_OUTPUT = 2
    }

    public class ConsoleEx
    {
        private const byte EMPTY = 0x20;
        private const int INVALID_HANDLE_VALUE = -1;
        private const int STD_INPUT_HANDLE = -10;
        private const int STD_OUTPUT_HANDLE = -11;
        private const int TITLE_LENGTH = 0x400;
        // Fields
        private static readonly CONSOLE_SCREEN_BUFFER_INFO ConsoleInfo;
        private static COORD ConsoleOutputLocation;
        private static int CurrentConsolePen;
        private static readonly int hConsoleInput = GetStdHandle(-10);
        private static readonly int hConsoleOutput = GetStdHandle(-11);
        private static readonly int OriginalConsolePen;

        // Methods
        static ConsoleEx()
        {
            if ((hConsoleOutput == -1) || (hConsoleInput == -1))
            {
                throw new ApplicationException("Unable to obtain buffer handle during initialization.");
            }
            ConsoleInfo = new CONSOLE_SCREEN_BUFFER_INFO();
            ConsoleOutputLocation = new COORD();
            GetConsoleScreenBufferInfo(hConsoleOutput, ref ConsoleInfo);
            OriginalConsolePen = ConsoleInfo.wAttributes;
            SetConsoleMode(hConsoleOutput, 1);
        }

        private ConsoleEx()
        {
            throw new NotSupportedException("This object may not be instantiated. Use static methods instead.");
        }

        // Properties
        public static int CursorHeight
        {
            get
            {
                var lpConsoleCursorInfo = new CONSOLE_CURSOR_INFO();
                GetConsoleCursorInfo(hConsoleOutput, ref lpConsoleCursorInfo);
                return lpConsoleCursorInfo.dwSize;
            }
            set
            {
                if ((value < 1) || (value > 100))
                {
                    throw new ArgumentOutOfRangeException("CursorHeight", value,
                        "Cursor height must be a percentage of the character cell between 1 and 100.");
                }
                var lpConsoleCursorInfo = new CONSOLE_CURSOR_INFO();
                GetConsoleCursorInfo(hConsoleOutput, ref lpConsoleCursorInfo);
                lpConsoleCursorInfo.dwSize = value;
                SetConsoleCursorInfo(hConsoleOutput, ref lpConsoleCursorInfo);
            }
        }

        public static bool CursorVisible
        {
            get
            {
                var lpConsoleCursorInfo = new CONSOLE_CURSOR_INFO();
                GetConsoleCursorInfo(hConsoleOutput, ref lpConsoleCursorInfo);
                return lpConsoleCursorInfo.bVisible;
            }
            set
            {
                var lpConsoleCursorInfo = new CONSOLE_CURSOR_INFO();
                GetConsoleCursorInfo(hConsoleOutput, ref lpConsoleCursorInfo);
                lpConsoleCursorInfo.bVisible = value;
                SetConsoleCursorInfo(hConsoleOutput, ref lpConsoleCursorInfo);
            }
        }

        public static short CursorX
        {
            get
            {
                var lpConsoleScreenBufferInfo = new CONSOLE_SCREEN_BUFFER_INFO();
                GetConsoleScreenBufferInfo(hConsoleOutput, ref lpConsoleScreenBufferInfo);
                return lpConsoleScreenBufferInfo.dwCursorPosition.x;
            }
            set { Move(value, CursorY); }
        }

        public static short CursorY
        {
            get
            {
                var lpConsoleScreenBufferInfo = new CONSOLE_SCREEN_BUFFER_INFO();
                GetConsoleScreenBufferInfo(hConsoleOutput, ref lpConsoleScreenBufferInfo);
                return lpConsoleScreenBufferInfo.dwCursorPosition.y;
            }
            set { Move(CursorX, value); }
        }

        public static string Title
        {
            get
            {
                var lpConsoleTitle = new StringBuilder(0x400);
                var consoleTitle = GetConsoleTitle(lpConsoleTitle, 0x400);
                return lpConsoleTitle.ToString(0, consoleTitle);
            }
            set
            {
                if (value.Length >= 0x400)
                {
                    throw new ArgumentOutOfRangeException("Title", value,
                        "Console window title must be no more than " + 0x400 + " characters.");
                }
                SetConsoleTitle(value);
            }
        }

        public static void Clear()
        {
            COORD coord;
            var lpNumberOfCharsWritten = 0;
            var lpConsoleScreenBufferInfo = new CONSOLE_SCREEN_BUFFER_INFO();
            coord.x = coord.y = 0;
            if (GetConsoleScreenBufferInfo(hConsoleOutput, ref lpConsoleScreenBufferInfo) == 0)
            {
                Console.Write('\f');
            }
            else
            {
                FillConsoleOutputCharacter(hConsoleOutput, 0x20,
                    lpConsoleScreenBufferInfo.dwSize.x*lpConsoleScreenBufferInfo.dwSize.y, coord,
                    ref lpNumberOfCharsWritten);
                FillConsoleOutputAttribute(hConsoleOutput, CurrentConsolePen,
                    lpConsoleScreenBufferInfo.dwSize.x*lpConsoleScreenBufferInfo.dwSize.y, coord,
                    ref lpNumberOfCharsWritten);
                SetConsoleCursorPosition(hConsoleOutput, coord);
            }
        }

        public static void DrawRectangle(BorderStyle style, int x, int y, int cx, int cy, bool fill)
        {
            string str;
            switch (style)
            {
                case BorderStyle.LineSingle:
                    str = "─│┌┐└┘";
                    break;

                case BorderStyle.LineDouble:
                    str = "═║╔╗╚╝";
                    break;

                case BorderStyle.None:
                    goto Label_00E3;

                default:
                    str = @"-|/\\/";
                    break;
            }
            var builder = new StringBuilder(cx + 1);
            builder.Append(str[2]);
            for (var i = 1; i < cx; i++)
            {
                builder.Append(str[0]);
            }
            builder.Append(str[3]);
            Move(x, y);
            Console.Write(builder);
            for (var j = 1; j < cy; j++)
            {
                Move(x, y + j);
                Console.Write(str[1]);
                Move(x + cx, y + j);
                Console.Write(str[1]);
            }
            builder[0] = str[4];
            builder[cx] = str[5];
            Move(x, y + cy);
            Console.Write(builder);
            Label_00E3:
            if (fill)
            {
                var lpNumberOfAttrsWritten = 0;
                var dwWriteCoord = new COORD
                {
                    x = (short) x
                };
                for (var k = 0; k <= cy; k++)
                {
                    dwWriteCoord.y = (short) (y + k);
                    FillConsoleOutputAttribute(hConsoleOutput, CurrentConsolePen, cx + 1, dwWriteCoord,
                        ref lpNumberOfAttrsWritten);
                }
            }
        }

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
            SetLastError = true)]
        private static extern int FillConsoleOutputAttribute(int hConsoleOutput, int wAttribute, int nLength,
            COORD dwWriteCoord, ref int lpNumberOfAttrsWritten);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
            SetLastError = true)]
        private static extern int FillConsoleOutputCharacter(int hConsoleOutput, byte cCharacter, int nLength,
            COORD dwWriteCoord, ref int lpNumberOfCharsWritten);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
            SetLastError = true)]
        private static extern int GetConsoleCursorInfo(int hConsoleOutput, ref CONSOLE_CURSOR_INFO lpConsoleCursorInfo);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
            SetLastError = true)]
        private static extern bool GetConsoleMode(int hConsoleHandle, ref int dwMode);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
            SetLastError = true)]
        private static extern int GetConsoleScreenBufferInfo(int hConsoleOutput,
            ref CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
            SetLastError = true)]
        private static extern int GetConsoleTitle(StringBuilder lpConsoleTitle, int nSize);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
            SetLastError = true)]
        private static extern int GetStdHandle(int nStdHandle);

        public static void Move(int x, int y)
        {
            COORD coord;
            var lpConsoleScreenBufferInfo = new CONSOLE_SCREEN_BUFFER_INFO();
            GetConsoleScreenBufferInfo(hConsoleOutput, ref lpConsoleScreenBufferInfo);
            if ((x >= lpConsoleScreenBufferInfo.dwSize.x) || (x < 0))
            {
                throw new ArgumentOutOfRangeException("x", x,
                    "The co-ordinates specified must be within the dimensions of the window.");
            }
            if ((y >= lpConsoleScreenBufferInfo.dwSize.y) || (y < 0))
            {
                throw new ArgumentOutOfRangeException("y", y,
                    "The co-ordinates specified must be within the dimensions of the window.");
            }
            coord.x = (short) x;
            coord.y = (short) y;
            SetConsoleCursorPosition(hConsoleOutput, coord);
        }

        public static char ReadChar()
        {
            SetConsoleMode(hConsoleInput, 0x19);
            var lpNumberOfCharsRead = 0;
            var buf = new StringBuilder(1);
            var flag = ReadConsole(hConsoleInput, buf, 1, ref lpNumberOfCharsRead, 0);
            SetConsoleMode(hConsoleInput, 0x1f);
            if (!flag)
            {
                throw new ApplicationException("Attempt to call ReadConsole API failed.");
            }
            return Convert.ToChar(buf[0]);
        }

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
            SetLastError = true)]
        private static extern bool ReadConsole(int hConsoleInput, StringBuilder buf, int nNumberOfCharsToRead,
            ref int lpNumberOfCharsRead, int lpReserved);

        public static void ResetColor()
        {
            SetConsoleTextAttribute(hConsoleOutput, OriginalConsolePen);
        }

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
            SetLastError = true)]
        private static extern int SetConsoleCursorInfo(int hConsoleOutput, ref CONSOLE_CURSOR_INFO lpConsoleCursorInfo);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
            SetLastError = true)]
        private static extern int SetConsoleCursorPosition(int hConsoleOutput, COORD dwCursorPosition);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
            SetLastError = true)]
        private static extern bool SetConsoleMode(int hConsoleHandle, int dwMode);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
            SetLastError = true)]
        private static extern int SetConsoleTextAttribute(int hConsoleOutput, int wAttributes);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
            SetLastError = true)]
        private static extern bool SetConsoleTitle(string lpConsoleTitle);

        public static void TextColor(ConsoleForeground foreground, ConsoleBackground background)
        {
            CurrentConsolePen = (int) foreground + (int) background;
            SetConsoleTextAttribute(hConsoleOutput, CurrentConsolePen);
        }

        public static void WriteAt(int x, int y, string text)
        {
            Move(x, y);
            Console.Write(text);
        }

        // Nested Types
        [StructLayout(LayoutKind.Sequential)]
        private struct CONSOLE_CURSOR_INFO
        {
            public int dwSize;
            public bool bVisible;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CONSOLE_SCREEN_BUFFER_INFO
        {
            public COORD dwSize;
            public COORD dwCursorPosition;
            public readonly int wAttributes;
            public readonly SMALL_RECT srWindow;
            public readonly COORD dwMaximumWindowSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct COORD
        {
            public short x;
            public short y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SMALL_RECT
        {
            public readonly short Left;
            public readonly short Top;
            public readonly short Right;
            public readonly short Bottom;
        }
    }
}