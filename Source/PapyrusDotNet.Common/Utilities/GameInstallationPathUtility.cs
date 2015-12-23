using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using PapyrusDotNet.Common.Enums;
using PapyrusDotNet.Common.Interfaces;

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