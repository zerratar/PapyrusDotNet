using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PapyrusDotNet.Core.Collections
{
	/// <summary>
	/// Example List<ValueType> converted class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class ListString : Form
	{
		private string[] ArrayHolder_0 = new string[128];
		private string[] ArrayHolder_1 = new string[128];
		private string[] ArrayHolder_2 = new string[128];
		private string[] ArrayHolder_3 = new string[128];
		private string[] ArrayHolder_4 = new string[128];
		private string[] ArrayHolder_5 = new string[128];
		private string[] ArrayHolder_6 = new string[128];
		private string[] ArrayHolder_7 = new string[128];
		private string[] ArrayHolder_8 = new string[128];
		private string[] ArrayHolder_9 = new string[128];

		private int ItemIndex;

		private int ItemCount;

		// Max = 1280;

		private int ArrayIndex(int bigIndex)
		{
			return bigIndex / 128;
		}

		private string[] ArrayFromIndex(int index)
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

		public void Add(string obj)
		{
			int iArray = ArrayIndex(ItemIndex);
			int offset = iArray * 128;
			int index = ItemIndex - offset;
			var array = ArrayFromIndex(iArray);
			array[index] = obj;
			ItemIndex++;
			ItemCount++;
		}
		public string Get(int index)
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

		public void Remove(string form)
		{

			//int f1Id = 0;

			for (int iArray = 0; iArray < 10; iArray++)
			{
				var arrayToCheck = ArrayFromIndex(iArray);
				for (int i = 0; i < 128; i++)
				{
					int x = 0;

					//if (arrayToCheck[i] != null)
					//{
					if (form == arrayToCheck[i])
					{
						arrayToCheck[i] = null;
						ItemCount--;
						return;
					}
					//}
				}
			}
		}

	}
}
