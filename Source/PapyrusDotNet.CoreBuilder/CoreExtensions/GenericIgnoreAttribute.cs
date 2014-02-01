using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PapyrusDotNet.CoreBuilder.CoreExtensions
{
	public class GenericIgnoreAttribute : Attribute
	{
		public GenericIgnoreAttribute()
		{
			// [GenericIgnore]
			// Used for members, fields and properties, etc.
		}
	}
}
