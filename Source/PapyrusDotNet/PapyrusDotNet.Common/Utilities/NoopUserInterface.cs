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

using PapyrusDotNet.Common.Interfaces;

namespace PapyrusDotNet.Common.Utilities
{
    public class NoopUserInterface : IUserInterface
    {
        public void Clear()
        {
        }

        public void DrawInterface(string message)
        {
        }

        public void DrawResult(string message)
        {
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
        }
    }
}