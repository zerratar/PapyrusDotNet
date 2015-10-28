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

using System.Collections.Generic;

namespace PapyrusDotNet.Common
{
    public class ObjectReplacementHolder<T, T2, T3>
    {
        public T Replacement { get; set; }
        public List<T2> ToReplace { get; set; } = new List<T2>();
        public List<T3> ToReplaceSecondary { get; set; } = new List<T3>();
    }
}
