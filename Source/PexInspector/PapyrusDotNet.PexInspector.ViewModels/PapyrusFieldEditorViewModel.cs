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
using PapyrusDotNet.Common.Utilities;
using PapyrusDotNet.PapyrusAssembly;

#endregion

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class PapyrusFieldEditorViewModel : PapyrusVariableParameterEditorViewModel
    {
        private object defaultValue;
        private readonly PapyrusFieldDefinition fieldToEdit;

        public PapyrusFieldEditorViewModel(IEnumerable<string> availableTypes, PapyrusFieldDefinition fieldToEdit = null)
            : base(availableTypes)
        {
            this.fieldToEdit = fieldToEdit;
            if (fieldToEdit != null)
            {
                Name = fieldToEdit.Name.Value;
                if (fieldToEdit.DefaultValue != null)
                {
                    if (fieldToEdit.DefaultValue.Value != null)
                    {
                        DefaultValue = fieldToEdit.DefaultValue.Value;
                    }
                }

                if (fieldToEdit.TypeName.Contains("[]"))
                    IsArray = true;

                var ft =
                    fieldToEdit.TypeName.ToLower();

                ft = ft.Replace("[]", "");

                SelectedType = TypeReferences.FirstOrDefault(t => t.ToString().ToLower() == ft);
                if (SelectedType == null)
                    SelectedType = fieldToEdit.TypeName.ToLower();
            }
        }

        public object DefaultValue
        {
            get { return defaultValue; }
            set { Set(ref defaultValue, value); }
        }

        public PapyrusVariableReference GetDefaultValue()
        {
            if (IsArray) return new PapyrusVariableReference();

            var type = Utility.GetPapyrusReturnType(SelectedTypeName);
            var primitiveType = Utility.GetPapyrusPrimitiveType(type);

            var defVal = fieldToEdit?.DefaultValue;
            if (defVal == null)
            {
                defVal = new PapyrusVariableReference();
            }

            defVal.Type = primitiveType;
            defVal.Value = Utility.ConvertToPapyrusValue(type, DefaultValue);

            return defVal;
        }
    }
}