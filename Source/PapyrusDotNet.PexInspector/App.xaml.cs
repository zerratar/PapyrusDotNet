using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PapyrusDotNet.PexInspector
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static bool OpenFileAtLaunch = false;
        public static string OpenFile = null;
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args != null && e.Args.Length > 0)
            {
                OpenFileAtLaunch = true;
                OpenFile = e.Args.FirstOrDefault();
            }
        }
    }
}
