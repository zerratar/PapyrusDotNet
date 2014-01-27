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
