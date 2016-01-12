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

#region

using PapyrusDotNet.Decompiler.Interfaces;

#endregion

namespace PapyrusDotNet.Decompiler
{
    public class PapyrusDecompilerResult : IPapyrusDecompilerResult
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusDecompilerResult" /> class.
        /// </summary>
        /// <param name="decompiledSourceCode">The decompiled source code.</param>
        /// <param name="errors">The errors.</param>
        public PapyrusDecompilerResult(string decompiledSourceCode, string errors)
        {
            Errors = errors;
            DecompiledSourceCode = decompiledSourceCode;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance has errors.
        /// </summary>
        public bool HasErrors => !string.IsNullOrEmpty(Errors);

        /// <summary>
        ///     Gets the errors.
        /// </summary>
        public string Errors { get; }

        /// <summary>
        ///     Gets the decompiled source code.
        /// </summary>
        public string DecompiledSourceCode { get; }
    }
}