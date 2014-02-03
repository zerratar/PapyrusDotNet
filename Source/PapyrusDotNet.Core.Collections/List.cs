namespace PapyrusDotNet.Core.Collections
{
	public class List<T>
	{
		public int Capacity;

		public int Length;

		public int Count;

		public ListStack<T>[] Arrays;

		public List()
		{
			Length = 0;
			Count = 0;
			Capacity = 128 * 128;
			Arrays = new ListStack<T>[128];
		}

		public void OnInit()
		{

			// Max length is 128.
			// But since we are creating arrays of arrays. (128*128) 
			// We are getting a max of 16384 items. More than enough?
			// Usual Array of Array: Item[][] but this is not supported
			// by Papyrus.
			Length = 0;
			Count = 0;
			Capacity = 128 * 128;
			Arrays = new ListStack<T>[128];
		}

		private int ArrayIndex(int bigIndex)
		{
			return bigIndex / 128;
		}


		public void Add(T item)
		{
			int arrayIndex = Length / 128;
			int itemIndex = Length - (arrayIndex * 128);
			Arrays[arrayIndex][itemIndex] = new ListItem<T>();
			Arrays[arrayIndex][itemIndex].ItemValue = item;
			Length++;
			Count++;
		}

	/*	public void Remove(T item)
		{
			var targetItem = new ListItem<T>();
			targetItem.ItemValue = item;

			for (int i = 0; i < Arrays.Length; i++)
			{
				for (int j = 0; j < Arrays[i].Items.Length; j++)
				{
					if (Arrays[i].Items[j] == targetItem)
					{
						Arrays[i].Items[j] = null;
						Count--;
						return;
					}
				}
			}
		}
		*/
		public void RemoveAt(int index)
		{
			int arrayIndex = index / 128;
			int itemIndex = index - (arrayIndex * 128);
			Arrays[arrayIndex][itemIndex] = null;
			Count--;
		}

		public T this[int index]
		{
			get
			{
				int arrayIndex = ArrayIndex(index);
				int offset = arrayIndex * 128;
				int i = index - offset;
				return Arrays[arrayIndex].Items[i].ItemValue;
			}
			set
			{
				int arrayIndex = ArrayIndex(index);
				int offset = arrayIndex * 128;
				int i = index - offset;
				Arrays[arrayIndex].Items[i].ItemValue = value;
			}
		}
	}

	public class ListStack<T>
	{
		public int Length;

		public int ItemCount;

		public ListItem<T>[] Items;

		// Constructors are
		// the same as OnInit
		// Except, if we want to test this
		// class we need to have both 
		// Constructor and OnInit doing
		// the same.
		public ListStack()
		{
			// Again, max 128 items.
			Items = new ListItem<T>[128];
		}

		public void OnInit()
		{
			Items = new ListItem<T>[128];
		}

		public ListItem<T> this[int index]
		{
			get
			{
				return Items[index];
			}
			set
			{
				Items[index] = value;
			}
		}
	}

	public class ListItem<T>
	{
		public T ItemValue;
	}
}
