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
	
	Copyright 2015, Karl Patrik Johansson, zerratar@gmail.com
 */
using System.IO;

namespace PapyrusDotNet.Common
{
    public class SkyrimInstallationPath
    {
        public bool SkyrimPathExists => !string.IsNullOrEmpty(Skyrim) && File.Exists(Skyrim);

        public bool CreationKitPathExists => !string.IsNullOrEmpty(CreationKit) && File.Exists(CreationKit);

        public string Skyrim { get; set; }

        public string CreationKit { get; set; }

        public SkyrimInstallationPath(string sp, string ckp)
        {
            if (Directory.Exists(sp))
                Skyrim = sp;

            if (Directory.Exists(ckp))
                CreationKit = ckp;
        }
    }
}