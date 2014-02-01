using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PapyrusDotNet.Common
{
	public class FieldProperties
	{
		public bool IsConditional { get; set; }
		public bool IsHidden { get; set; }
		public bool IsProperty { get; set; }
		public bool IsAuto { get; set; }
		public bool IsAutoReadOnly { get; set; }
		public bool IsGeneric { get; set; }
		public string InitialValue { get; set; }
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
