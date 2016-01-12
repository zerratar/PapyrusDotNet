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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace PapyrusDotNet.Decompiler.HelperClasses
{
    public class Map<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly SortedDictionary<TKey, TValue> dict = new SortedDictionary<TKey, TValue>();

        /// <summary>
        ///     Gets or sets the <see cref="TValue" /> with the specified key.
        ///     And adds a new record if the key did not previously exist.
        /// </summary>
        /// <value>
        ///     The <see cref="TValue" />.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get
            {
                if (!dict.ContainsKey(key))
                    dict.Add(key, default(TValue));

                return dict[key];
            }
            set
            {
                if (dict.ContainsKey(key))
                    dict[key] = value;
                else
                    dict.Add(key, value);
            }
        }

        /// <summary>
        ///     Gets the size of this Map.
        /// </summary>
        public int Size => dict.Count;

        /// <summary>
        ///     Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        /// <summary>
        ///     Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Gets the available keys.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TKey> GetKeys() => dict.Keys;

        /// <summary>
        ///     Gets the available values.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TValue> GetValues() => dict.Values;

        /// <summary>
        ///     Finds the item specified by a key without adding a new record.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public TValue Find(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (dict.ContainsKey(key))
                return dict[key];
            return default(TValue);
        }

        /// <summary>
        ///     Finds the key using the value without adding a new record.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public TKey FindKey(TValue item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            foreach (var i in dict)
            {
                if (i.Value != null && i.Value.Equals(item))
                {
                    return i.Key;
                }
            }

            throw new KeyNotFoundException(
                "The requested key could not be found in part of this collection. Make sure you pass a long an existing Value.");
        }

        /// <summary>
        ///     Tries to the find key using the value without adding a new record.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool TryFindKey(TValue item, out TKey key)
        {
            try
            {
                key = FindKey(item);
                return true;
            }
            catch
            {
            }

            key = default(TKey);
            return false;
        }

        /// <summary>
        ///     Erases the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Erase(TKey key)
        {
            dict.Remove(key);
        }

        public TValue GetNext(TValue it)
        {
            var key = FindKey(it);
            var keyArray = dict.Keys.ToArray();
            var index = Array.IndexOf(keyArray, key);
            if (index + 1 < Size)
            {
                return dict[keyArray[index + 1]];
            }
            return default(TValue);
        }

        /// <summary>
        ///     Clears this instance.
        /// </summary>
        public void Clear()
        {
            dict.Clear();
        }
    }
}