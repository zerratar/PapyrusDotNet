using System.Collections.Generic;
using System.Linq;
using PapyrusDotNet.Common.Utilities;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Extensions;

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class PapyrusFieldEditorViewModel : PapyrusVariableParameterEditorViewModel
    {
        public PapyrusFieldEditorViewModel(IEnumerable<string> availableTypes, PapyrusFieldDefinition fieldToEdit = null) : base(availableTypes)
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

        private object defaultValue;
        private PapyrusFieldDefinition fieldToEdit;

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

            defVal.ValueType = primitiveType;
            defVal.Value = Utility.ConvertToPapyrusValue(type, DefaultValue);

            return defVal;
        }
    }
}