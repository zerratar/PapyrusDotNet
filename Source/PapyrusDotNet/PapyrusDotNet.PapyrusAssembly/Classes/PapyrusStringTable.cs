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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusStringTable : IEnumerable<PapyrusStringTableIndex>
    {
        private readonly List<PapyrusStringTableIndex> rows = new List<PapyrusStringTableIndex>();

        private readonly Dictionary<string, PapyrusStringTableIndex> rowData =
            new Dictionary<string, PapyrusStringTableIndex>();

        /// <summary>
        ///     Gets or sets the size.
        /// </summary>
        public int Size => rows.Count;

        /// <summary>
        ///     Gets or sets the <see cref="Index" /> at the specified index.
        /// </summary>
        /// <value>
        ///     The <see cref="Index" />.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public PapyrusStringTableIndex this[int index]
        {
            get { return rows.FirstOrDefault(r => r.TableIndex == index); }
            set
            {
                var existing = rows.FirstOrDefault(r => r.TableIndex == index);
                if (existing != null)
                {
                    var i = rows.IndexOf(existing);
                    rows[i] = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="Index" /> with the specified identifier.
        /// </summary>
        /// <value>
        ///     The <see cref="Index" />.
        /// </value>
        /// <param name="identifier">The identifier.</param>
        /// <returns></returns>
        public PapyrusStringTableIndex this[string identifier]
        {
            get
            {
                var idx = rows.FirstOrDefault(r => r.Identifier.ToLower() == identifier.ToLower());
                return idx;
            }
            set
            {
                var existing = rows.FirstOrDefault(r => r.Identifier.ToLower() == identifier.ToLower());
                if (existing != null)
                {
                    var i = rows.IndexOf(existing);
                    rows[i] = value;
                }
            }
        }

        public IEnumerator<PapyrusStringTableIndex> GetEnumerator()
        {
            return rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(string identifier)
        {
            var item =
                rows.FirstOrDefault(i => i.Identifier.ToLower() == identifier.ToLower());
            if (item == null) return -1;
            return rows.IndexOf(item);
        }

        /// <summary>
        /// Adds the specified identifier to the string table, if it already exists it will just return the existing one.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="forceAddAlreadyExists">if set to <c>true</c> [force add already exists].</param>
        /// <returns></returns>
        public PapyrusStringTableIndex Add(string identifier, bool forceAddAlreadyExists = false)
        {
            if (rowData.ContainsKey(identifier.ToLower()) && !forceAddAlreadyExists)
                return rowData[identifier.ToLower()];

            //var existing =
            //rows.FirstOrDefault(i => i.Identifier.ToLower() == identifier.ToLower());
            //if (existing != null) return existing;

            var papyrusStringTableIndex = new PapyrusStringTableIndex(this, Size, identifier);

            rows.Add(papyrusStringTableIndex);

            if (!rowData.ContainsKey(identifier.ToLower()))
                rowData.Add(identifier.ToLower(), papyrusStringTableIndex);

            return papyrusStringTableIndex;
        }
    }
}