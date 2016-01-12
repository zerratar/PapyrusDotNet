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

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusStringTableIndex
    {
        /// <summary>
        ///     The undefined identifier
        /// </summary>
        public const string UndefinedIdentifier = "Undefined";

        /// <summary>
        ///     The undefined index
        /// </summary>
        public const int UndefinedIndex = 0xffff;

        /// <summary>
        ///     The undefined
        /// </summary>
        public static PapyrusStringTableIndex Undefined = new PapyrusStringTableIndex();

        /// <summary>
        ///     The table
        /// </summary>
        private readonly PapyrusStringTable table;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Index" /> class.
        /// </summary>
        /// <param name="tableIndex">Index of the table.</param>
        /// <param name="identifier">The identifier.</param>
        public PapyrusStringTableIndex(int tableIndex, string identifier)
        {
            Identifier = identifier;
            TableIndex = tableIndex;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Index" /> class.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="tableIndex">Index of the table.</param>
        /// <param name="identifier">The identifier.</param>
        public PapyrusStringTableIndex(PapyrusStringTable table, int tableIndex, string identifier)
            : this(tableIndex, identifier)
        {
            this.table = table;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Index" /> class.
        /// </summary>
        public PapyrusStringTableIndex() : this(null, 0, UndefinedIdentifier)
        {
        }

        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        ///     Gets or sets the index of the table.
        /// </summary>
        /// <value>
        ///     The index of the table.
        /// </value>
        public int TableIndex { get; set; }

        /// <summary>
        ///     Gets the table.
        /// </summary>
        /// <returns></returns>
        public PapyrusStringTable GetTable() => table;

        /// <summary>
        ///     Determines whether this instance is valid.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return (TableIndex == UndefinedIndex) || (table != null && TableIndex < table.Size);
        }

        /// <summary>
        ///     Determines whether this instance is undefined.
        /// </summary>
        /// <returns></returns>
        public bool IsUndefined()
        {
            return IsValid() && TableIndex == UndefinedIndex;
        }

        /// <summary>
        ///     Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        protected bool Equals(PapyrusStringTableIndex other)
        {
            return Equals(table, other.table) && string.Equals(Identifier, other.Identifier) &&
                   TableIndex == other.TableIndex;
        }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PapyrusStringTableIndex) obj);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = table != null ? table.GetHashCode() : 0;
                hashCode = (hashCode*397) ^ (Identifier != null ? Identifier.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ TableIndex;
                return hashCode;
            }
        }

        /// <summary>
        ///     Implements the operator ==.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static bool operator ==(PapyrusStringTableIndex a, PapyrusStringTableIndex b)
        {
            if (ReferenceEquals(null, a) && !ReferenceEquals(null, b)) return false;
            if (!ReferenceEquals(null, a) && ReferenceEquals(null, b)) return false;
            if (ReferenceEquals(null, a)) return true;

            return a.Identifier.ToLower() == b.Identifier.ToLower() ||
                   a.table == b.table && a.TableIndex == b.TableIndex;
        }

        /// <summary>
        ///     Implements the operator !=.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static bool operator !=(PapyrusStringTableIndex a, PapyrusStringTableIndex b)
        {
            if (ReferenceEquals(null, a) && !ReferenceEquals(null, b)) return true;
            if (!ReferenceEquals(null, a) && ReferenceEquals(null, b)) return true;
            if (ReferenceEquals(null, a)) return false;

            return a.table != b.table || a.TableIndex != b.TableIndex;
        }

        /// <summary>
        ///     Implements the operator explicit string.
        /// </summary>
        /// <param name="r">The table index reference.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static explicit operator string(PapyrusStringTableIndex r)
        {
            return r?.Identifier;
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Identifier;
        }
    }
}