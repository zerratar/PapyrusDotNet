//     This file is part of PapyrusDotNet.
// 
//     PapyrusDotNet is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     PapyrusDotNet is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with PapyrusDotNet.  If not, see <http://www.gnu.org/licenses/>.
//  
//     Copyright 2016, Karl Patrik Johansson, zerratar@gmail.com

#region

using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using PapyrusDotNet.Common.Enums;
using PapyrusDotNet.Common.Interfaces;

#endregion

namespace PapyrusDotNet.Common.Utilities
{
    public class GameInstallationPathUtility : IUtility
    {
        private readonly Games game;

        /*[HKEY_CLASSES_ROOT\Local Settings\Software\Microsoft\Windows\Shell\MuiCache]
       "D:\\The Elder Scrolls V Skyrim\\CreationKit.exe"="Creation Kit"
       "D:\\The Elder Scrolls V Skyrim\\TESV.exe"="Skyrim"
       "D:\\The Elder Scrolls V Skyrim\\SkyrimLauncher.exe"="Skyrim Launcher"
       "D:\\The Elder Scrolls V Skyrim\\unins000.exe"="Setup/Uninstall"*/

        public GameInstallationPathUtility(Games game)
        {
            this.game = game;
        }

        public SkyrimInstallationPath GetInstallationFolder()
        {
            try
            {
                var subLocalSettings = Registry.ClassesRoot.OpenSubKey("Local Settings");
                var subSoftware = subLocalSettings?.OpenSubKey("Software");
                var subMicrosoft = subSoftware?.OpenSubKey("Microsoft");
                var subWindows = subMicrosoft?.OpenSubKey("Windows");
                var subShell = subWindows?.OpenSubKey("Shell");
                var subMuiCache = subShell?.OpenSubKey("MuiCache");
                var test = subMuiCache?.GetValueNames();

                var gameExe = game == Games.Fallout4 ? "fallout4.exe" : "tesv.exe";
                var launcherExe = game == Games.Fallout4 ? "fallout4launcher.exe" : "skyrimlauncher.exe";
                var installationFolder =
                    test?.FirstOrDefault(
                        s => s.ToLower().Contains(gameExe) || s.ToLower().Contains(launcherExe));

                var creationkitFolder = test?.FirstOrDefault(s => s.ToLower().Contains("creationkit.exe"));

                var ifo = "";
                var ckfo = "";

                if (!string.IsNullOrEmpty(installationFolder))
                    ifo = Path.GetDirectoryName(installationFolder);

                if (!string.IsNullOrEmpty(creationkitFolder))
                    ckfo = Path.GetDirectoryName(creationkitFolder);

                return new SkyrimInstallationPath(ifo, ckfo);
            }
            catch (Exception)
            {
                // Ignored
            }
            return null;
        }
    }
}