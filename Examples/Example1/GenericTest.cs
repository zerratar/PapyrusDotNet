using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Example1
{
	using PapyrusDotNet.Core;

	public class GenericTest
	{
		private GenericClass<bool> boolgen;

		[Property, Auto]
		public GenericClass<bool> boolgenProp; 


		public void OnInit()
		{
			GenericClass<Form> formGeneric = new GenericClass<Form>();
			formGeneric.Set(null);

			GenericClass<int> intGeneric = new GenericClass<int>();
			intGeneric.Set(9999);
		}
	}

	public class GenericClass<T>
	{
		public T GenericVariable;

		public void Set(T value)
		{
			GenericVariable = value;
		}

		public T Get()
		{
			return GenericVariable;
		}
	}
}
