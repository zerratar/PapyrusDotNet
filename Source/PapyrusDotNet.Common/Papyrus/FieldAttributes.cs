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
//     Copyright 2015, Karl Patrik Johansson, zerratar@gmail.com

namespace PapyrusDotNet.Common.Papyrus
{
    public class FieldAttributes
    {
        public bool IsConditional { get; set; }
        public bool IsHidden { get; set; }
        public bool IsProperty { get; set; }
        public bool IsAuto { get; set; }
        public bool IsAutoReadOnly { get; set; }
        public bool IsGeneric { get; set; }
        public string InitialValue { get; set; }
        public string DocString { get; set; }

        public int UserFlagsValue
        {
            get
            {
                var output = 0;
                if (IsHidden) output += 2;
                if (IsConditional) output += 1;
                return output;
            }
        }
    }
}