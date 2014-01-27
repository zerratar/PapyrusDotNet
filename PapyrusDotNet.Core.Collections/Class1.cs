using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PapyrusDotNet.Core.Collections
{
	public class List<T> where T : class
	{
		private T[] ArrayHolder_0 = new T[128];
		private T[] ArrayHolder_1 = new T[128];
		private T[] ArrayHolder_2 = new T[128];
		private T[] ArrayHolder_3 = new T[128];
		private T[] ArrayHolder_4 = new T[128];
		private T[] ArrayHolder_5 = new T[128];
		private T[] ArrayHolder_6 = new T[128];
		private T[] ArrayHolder_7 = new T[128];
		private T[] ArrayHolder_8 = new T[128];
		private T[] ArrayHolder_9 = new T[128];

		private int ItemIndex;

		private int ItemCount;

		// Max = 1280;

		private int ArrayIndex(int bigIndex)
		{
			return bigIndex / 128;
		}

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
			
			array[i] = null;
			ItemCount--;
		}

		public void Remove(T form)
		{

			//int f1Id = 0;
			
			for (int iArray = 0; iArray < 10; iArray++)
			{
				var arrayToCheck = ArrayFromIndex(iArray);
				for (int i = 0; i < 128; i++)
				{
					if (arrayToCheck[i] != null)
					{
						//var formId = arrayToCheck[i].GetFormID();
						//if (f1Id == formId)
						//{
						//	arrayToCheck[i] = null;
						//	ItemCount--;
						//	return;
						//}

						if (form == arrayToCheck[i])
						{
							arrayToCheck[i] = null;
							ItemCount--;
							return;
						}
					}
				}
			}
		}

	}
}
