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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PapyrusDotNet.Core.Collections
{
	/// <summary>
	/// This class is for testing only, to test and see the different ways to 'implement' Generics for Papyrus
	/// Since Generics are generated on Runtime on the .NET Framework, which is not supported by Skyrim.
	/// Possible work-arounds are to act more like a Template in C/C++ where the "T" is generated on Compile time.
	/// It means that for the PapyrusDotNet .NET -> Papyrus project we would need to generate extra code for each Generic type used
	/// Replacing the Generic Type into a ValueType or Class.
	/// 
	/// 
	/// Ex: List<ObjectReference>, we would need to generate a ListObjectReference class that will automatically be used instead.
	/// That way we can get it to work!
	/// 
	/// 
	/// See the other List classes for references.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// 
	[GenericType("List")]
	public class List<T>
	{
		[GenericMember]
		private T[] ArrayHolder_0 = new T[128];
		[GenericMember]
		private T[] ArrayHolder_1 = new T[128];
		[GenericMember]
		private T[] ArrayHolder_2 = new T[128];
		[GenericMember]
		private T[] ArrayHolder_3 = new T[128];
		[GenericMember]
		private T[] ArrayHolder_4 = new T[128];
		[GenericMember]
		private T[] ArrayHolder_5 = new T[128];
		[GenericMember]
		private T[] ArrayHolder_6 = new T[128];
		[GenericMember]
		private T[] ArrayHolder_7 = new T[128];
		[GenericMember]
		private T[] ArrayHolder_8 = new T[128];
		[GenericMember]
		private T[] ArrayHolder_9 = new T[128];

		private int ItemIndex;

		private int ItemCount;

		// Max = 1280;

		private int ArrayIndex(int bigIndex)
		{
			return bigIndex / 128;
		}

		[GenericMember]
		private T[] ArrayFromIndex(int index)
		{
			if (index == 0) return ArrayHolder_0;
			if (index == 1) return ArrayHolder_1;
			if (index == 2) return ArrayHolder_2;
			if (index == 3) return ArrayHolder_3;
			if (index == 4) return ArrayHolder_4;
			if (index == 5) return ArrayHolder_5;
			if (index == 6) return ArrayHolder_6;
			if (index == 7) return ArrayHolder_7;
			if (index == 8) return ArrayHolder_8;
			if (index == 9) return ArrayHolder_9;
			return ArrayHolder_0;
		}

		public int Count()
		{	
			return ItemCount;
		}

		public int Size()
		{
			return ItemIndex;
		}

		[GenericMember]
		public void Add(T obj)
		{
			int iArray = ArrayIndex(ItemIndex);
			int offset = iArray * 128;
			int index = ItemIndex - offset;
			var array = ArrayFromIndex(iArray);
			array[index] = obj;
			ItemIndex++;
			ItemCount++;
		}

		[GenericMember]
		public T Get(int index)
		{
			int iArray = ArrayIndex(index);
			int offset = iArray * 128;
			int i = index - offset;
			var array = ArrayFromIndex(iArray);
			return array[i];
		}


		public void RemoveAt(int index)
		{
			int iArray = ArrayIndex(index);
			int offset = iArray * 128;
			int i = index - offset;
			var array = ArrayFromIndex(iArray);
			
			// array[i] = null;
			
			ItemCount--;
		}

		[GenericMember]
		public void Remove(T form)
		{

			//int f1Id = 0;
			
			for (int iArray = 0; iArray < 10; iArray++)
			{
				var arrayToCheck = ArrayFromIndex(iArray);
				for (int i = 0; i < 128; i++)
				{
					int x = 0;
					
					if (arrayToCheck[i] != null)
					{
						if (form.Equals(arrayToCheck[i]))
						{
							// arrayToCheck[i] = null;
							ItemCount--;
							return;
						}
					}
				}
			}
		}

	}
}
