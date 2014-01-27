using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PapyrusDotNet.CoreBuilder
{
	using PowerArgs;

	public class PapyrusDotNetArgs
	{
		[ArgShortcut("type")]
		[ArgShortcut("t")]
		[ArgPosition(1)]
		public string InputType { get; set; }

		[ArgShortcut("input")]
		[ArgShortcut("i")]
		[ArgPosition(0)]
		public string InputFolder { get; set; }
	}
}
