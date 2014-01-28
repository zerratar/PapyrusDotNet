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
	
	Copyright 2014, Karl Patrik Johansson, zerratar@gmail.com
 */

namespace PapyrusDotNet.Tester
{
	using PapyrusDotNet.Core;
	using PapyrusDotNet.Core.Collections;

	public class Program
	{
		
		static void Main(string[] args)
		{
			
			var list = new List<Form>();
			var f1 = new Form();
			
			list.Add(f1);
			list.Add(f1);
			list.Add(f1);
			list.Add(f1);

			var f2 = list.Get(1);

			for (int j = 0; j < list.Size(); j++)
			{
				var item = list.Get(j);
			}
		}
	}
}
