namespace PapyrusDotNet.Core.Collections
{
	using PapyrusDotNet.Core;

	public class Collection
	{
		public int Length;

		public CollectionArray[] Arrays;

		public void OnInit()
		{
			
			// Max length is 128.
			// But since we are creating arrays of arrays. (128*128) 
			// We are getting a max of 16384 items. More than enough?
			// Usual Array of Array: Item[][] but this is not supported
			// by Papyrus.
			Length = 128 * 128;
			Arrays = new CollectionArray[128];
		}

		private int ArrayIndex(int bigIndex)
		{
			return bigIndex / 128;
		}

		public CollectionArrayItem this[int index]
		{
			get
			{
				int arrayIndex = ArrayIndex(index);
				int offset = arrayIndex * 128;
				int i = index - offset;
				return Arrays[arrayIndex].Items[i];
			}
			set
			{
				int arrayIndex = ArrayIndex(index);
				int offset = arrayIndex * 128;
				int i = index - offset;
				Arrays[arrayIndex].Items[i] = value;
			}
		}

		public CollectionArrayItem this[int arrayIndex, int itemIndex]
		{
			get
			{
				return Arrays[arrayIndex].Items[itemIndex];
			}
			set
			{
				Arrays[arrayIndex].Items[itemIndex] = value;
			}
		}

	}

	public class CollectionArray
	{
		public int Length;

		public int ItemCount;

		public CollectionArrayItem[] Items;

		// Constructors are
		// the same as OnInit
		public CollectionArray()
		{
			// Again, max 128 items.
			Items = new CollectionArrayItem[128];
		}

		public int GetInt(int index)
		{
			return Items[index].IntValue;
		}
		public Form GetForm(int index)
		{
			return Items[index].FormValue;
		}
		public float GetFloat(int index)
		{
			return Items[index].FloatValue;
		}
		public bool GetBool(int index)
		{
			return Items[index].BoolValue;
		}

		public int AddInt(int val)
		{
			var item = new CollectionArrayItem();
			item.IntValue = val;
			Items[ItemCount] = item;
			return ItemCount++;
		}

		public int AddBool(bool val)
		{
			var item = new CollectionArrayItem();
			item.BoolValue = val;
			Items[ItemCount] = item;
			return ItemCount++;
		}

		public int AddFloat(float val)
		{
			var item = new CollectionArrayItem();
			item.FloatValue = val;
			Items[ItemCount] = item;
			return ItemCount++;
		}

		public int AddForm(Form val)
		{
			var item = new CollectionArrayItem();
			item.FormValue = val;
			Items[ItemCount] = item;
			return ItemCount++;
		}

	}

	public class CollectionArrayItem
	{
		public float FloatValue;

		public int IntValue;

		public bool BoolValue;

		public string StringValue;

		public Form FormValue;
	}
}
