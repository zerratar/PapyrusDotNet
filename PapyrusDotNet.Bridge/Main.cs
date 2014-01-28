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

namespace PapyrusDotNet.Bridge
{
	using System.Runtime.InteropServices;
	using System.Threading;

	using EasyHook;

	using PapyrusDotNet.Launcher;

	public class Main : EasyHook.IEntryPoint
	{
		private SkyrimInterface Interface;
		private Stack<String> Queue = new Stack<String>();

		private LocalHook FunctionHook;

		public Main(
		RemoteHooking.IContext InContext,
		String InChannelName)
		{
			// connect to host...
			Interface = RemoteHooking.IpcConnectClient<SkyrimInterface>(InChannelName);

			Interface.IntensiveThingHere("Hello World!");
			// Interface.Ping();
		}

		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
		private delegate IntPtr Test();


		IntPtr TestFunc()
		{
			return (IntPtr)0;
		}

		public void Run(RemoteHooking.IContext InContext, String InChannelName)
		{
			// install hook...
			try
			{

				FunctionHook = LocalHook.Create(
					LocalHook.GetProcAddress("kernel32.dll", "CreateFileW"),
					new Test(TestFunc), // new DCreateFile(CreateFile_Hooked)					
					this);

				FunctionHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
			}
			catch (Exception ExtInfo)
			{
				Interface.ReportException(ExtInfo);

				return;
			}

			Interface.IsInstalled(RemoteHooking.GetCurrentProcessId());

			RemoteHooking.WakeUpProcess();

			// wait for host process termination...
			try
			{
				while (true)
				{
					Thread.Sleep(500);

					//// transmit newly monitored file accesses...
					//if (Queue.Count > 0)
					//{
					//	String[] Package = null;

					//	lock (Queue)
					//	{
					//		Package = Queue.ToArray();

					//		Queue.Clear();
					//	}

					//	Interface.OnCreateFile(RemoteHooking.GetCurrentProcessId(), Package);
					//}
					//else
					//	Interface.Ping();
				}
			}
			catch
			{
				// Ping() will raise an exception if host is unreachable
			}
		}
	}
}
