/*
    This file is part of PapyrusDotNet.

    PapyrusDotNet is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    PapyrusDotNet is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with PapyrusDotNet.  If not, see <http://www.gnu.org/licenses/>.
	
	Copyright 2014, Karl Patrik Johansson, zerratar@gmail.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PapyrusDotNet.Launcher
{
	using System.Runtime.Remoting;

	using EasyHook;

	class Program
	{
		static String ChannelName = null;

		static void Main(string[] args)
		{
			Int32 TargetPID = 0;

			TargetPID = 11960;

			if (TargetPID == 0)
			{
				if ((args.Length != 1) || !Int32.TryParse(args[0], out TargetPID))
				{

					Console.WriteLine();
					Console.WriteLine("Usage: PapyrusDotNet.Launcher %PID%");
					Console.WriteLine();

					return;
				}
			}

			try
			{
				try
				{
					Config.Register(
						"A Bridge between Skyrim and PapyrusDotNet",
						"PapyrusDotNet.Launcher.exe",
						"PapyrusDotNet.Bridge.dll"
						);
				}
				catch (ApplicationException exc)
				{
					// MessageBox.Show("This is an administrative task!", "Permission denied...", MessageBoxButtons.OK);
					Console.WriteLine(exc.ToString());

					System.Diagnostics.Process.GetCurrentProcess().Kill();
				}

				RemoteHooking.IpcCreateServer<SkyrimInterface>(ref ChannelName, WellKnownObjectMode.SingleCall);

				RemoteHooking.Inject(
					TargetPID,
					"PapyrusDotNet.Bridge.dll",
					"PapyrusDotNet.Bridge.dll",
					ChannelName);

				Console.ReadLine();
			}
			catch (Exception ExtInfo)
			{
				Console.WriteLine("There was an error while connecting to target:\r\n{0}", ExtInfo.ToString());
			}
		}
	}
}
