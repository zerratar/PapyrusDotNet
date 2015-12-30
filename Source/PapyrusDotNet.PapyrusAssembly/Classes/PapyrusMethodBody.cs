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

#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace PapyrusDotNet.PapyrusAssembly
{
    public class PapyrusMethodBody
    {
        private readonly PapyrusMethodDefinition method;

        public PapyrusMethodBody(PapyrusMethodDefinition method)
        {
            this.method = method;
            Instructions = new PapyrusInstructionCollection();
            Variables = new List<PapyrusVariableReference>();
            //TempVariables = new List<PapyrusVariableReference>();
        }

        public bool HasVariables => Variables.Any();
        public bool IsEmpty => !Instructions.Any();

        public List<PapyrusVariableReference> Variables { get; set; }

        //public List<PapyrusVariableReference> TempVariables { get; set; }

        public PapyrusInstructionCollection Instructions { get; set; }

        public PapyrusMethodDefinition GetMethod() => method;
    }
}