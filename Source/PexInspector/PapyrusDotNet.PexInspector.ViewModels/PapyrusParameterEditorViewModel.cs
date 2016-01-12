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

using System.Collections.Generic;
using System.Linq;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class PapyrusParameterEditorViewModel : PapyrusVariableParameterEditorViewModel
    {
        private readonly PapyrusParameterDefinition parameter;

        public PapyrusParameterEditorViewModel(IEnumerable<string> types, PapyrusParameterDefinition parameter = null)
            : base(types)
        {
            this.parameter = parameter;

            if (this.parameter != null)
            {
                Name = this.parameter.Name.Value;

                if (parameter.TypeName.Value.Contains("[]"))
                    IsArray = true;

                var ft =
                    parameter.TypeName.Value.ToLower();

                ft = ft.Replace("[]", "");

                SelectedType = TypeReferences.FirstOrDefault(t => t.ToString().ToLower() == ft);
                if (SelectedType == null)
                    SelectedType = this.parameter.TypeName.Value.ToLower();
            }
        }
    }
}