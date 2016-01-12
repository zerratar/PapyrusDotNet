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

using PapyrusDotNet.Decompiler.Node;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.Decompiler
{
    public class PapyrusCodeBlock
    {
        /// <summary>
        ///     The End
        /// </summary>
        /// <remarks>No puns intended</remarks>
        public static int END = int.MaxValue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PapyrusCodeBlock" /> class.
        /// </summary>
        /// <param name="begin">The begin.</param>
        /// <param name="end">The end.</param>
        public PapyrusCodeBlock(int begin, int end)
        {
            Begin = begin;
            End = end;
            Next = END;
            OnFalse = END;
            Scope = new ScopeNode();
        }

        /// <summary>
        ///     Gets or sets the begin.
        /// </summary>
        public int Begin { get; set; }

        /// <summary>
        ///     Gets or sets the end.
        /// </summary>
        public int End { get; set; }

        /// <summary>
        ///     Gets or sets the next.
        /// </summary>
        public int Next { get; set; }

        /// <summary>
        ///     Gets or sets the on false.
        /// </summary>
        public int OnFalse { get; set; }

        /// <summary>
        ///     Gets the on true.
        /// </summary>
        /// <value>
        ///     The on true.
        /// </value>
        public int OnTrue => Next;

        /// <summary>
        ///     Gets or sets the scope.
        /// </summary>
        public ScopeNode Scope { get; set; }

        /// <summary>
        ///     Gets or sets the condition.
        /// </summary>
        public PapyrusStringTableIndex Condition { get; set; }

        /// <summary>
        ///     Determines whether this instance is conditional.
        /// </summary>
        /// <returns></returns>
        public bool IsConditional()
        {
            return Condition != null && Condition.IsValid() && !Condition.IsUndefined();
        }

        /// <summary>
        ///     Splits the specified split.
        /// </summary>
        /// <param name="split">The split.</param>
        /// <returns></returns>
        public PapyrusCodeBlock Split(int split)
        {
            var result = new PapyrusCodeBlock(split, End);
            result.Next = Next;
            result.Condition = Condition;
            result.OnFalse = OnFalse;

            End = split - 1;
            Next = split;
            Condition = new PapyrusStringTableIndex();
            OnFalse = END;
            return result;
        }

        /// <summary>
        ///     Sets the condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="ontrue">The ontrue.</param>
        /// <param name="onfalse">The onfalse.</param>
        public void SetCondition(PapyrusStringTableIndex condition, int ontrue, int onfalse)
        {
            Condition = condition;
            Next = ontrue;
            OnFalse = onfalse;
        }
    }
}