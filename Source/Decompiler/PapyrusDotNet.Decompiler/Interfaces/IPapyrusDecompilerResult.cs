//     This file is part of PapyrusDotNet.
//     But is a port of Champollion, https://github.com/Orvid/Champollion
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
//     Copyright © 2016, Karl Patrik Johansson, zerratar@gmail.com
//     Copyright © 2015, Orvid King
//     Copyright © 2013, Paul-Henry Perrin

namespace PapyrusDotNet.Decompiler.Interfaces
{
    public interface IPapyrusDecompilerResult
    {
        /// <summary>
        ///     Gets a value indicating whether this instance has errors.
        /// </summary>
        bool HasErrors { get; }

        /// <summary>
        ///     Gets the errors.
        /// </summary>
        string Errors { get; }

        /// <summary>
        ///     Gets the decompiled source code.
        /// </summary>
        string DecompiledSourceCode { get; }
    }
}