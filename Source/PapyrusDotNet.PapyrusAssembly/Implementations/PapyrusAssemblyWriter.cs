#region License

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
//     Copyright 2015, Karl Patrik Johansson, zerratar@gmail.com

#endregion

#region

using System;
using System.IO;
using PapyrusDotNet.PapyrusAssembly.Interfaces;
using PapyrusDotNet.PapyrusAssembly.IO;

#endregion

namespace PapyrusDotNet.PapyrusAssembly.Implementations
{
    internal class PapyrusAssemblyWriter : IPapyrusAssemblyWriter, IDisposable
    {
        private readonly PexWriter pexWriter;
        private bool isDisposed;

        public PapyrusAssemblyWriter()
        {
            pexWriter = new PexWriter(new MemoryStream());
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Write(string outputFile)
        {
        }

        ~PapyrusAssemblyWriter()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;
            if (disposing)
            {
                pexWriter.Dispose();
            }
            isDisposed = true;
        }
    }
}