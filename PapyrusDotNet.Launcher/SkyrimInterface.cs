using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PapyrusDotNet.Launcher
{
	public class SkyrimInterface : MarshalByRefObject
	{
		public void IsInstalled(Int32 InClientPID)
		{
			Console.WriteLine("PapyrusDotNet.Bridge has been installed in target {0}.\r\n", InClientPID);
		}

		public void IntensiveThingHere(string val)
		{
			Console.WriteLine(val);
		}

		public void ReportException(Exception ExtInfo)
		{
			throw new NotImplementedException();
		}
	}
}
