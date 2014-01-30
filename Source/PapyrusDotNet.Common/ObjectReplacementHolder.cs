using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PapyrusDotNet.Common
{

	public class ObjectReplacementHolder<T, T2, T3>
	{
		public T Replacement { get; set; }
		public List<T2> ToReplace { get; set; }
		public List<T3> ToReplaceSecondary { get; set; }
		public ObjectReplacementHolder()
		{
			ToReplace = new List<T2>();
			ToReplaceSecondary = new List<T3>();
		}
	}
}
