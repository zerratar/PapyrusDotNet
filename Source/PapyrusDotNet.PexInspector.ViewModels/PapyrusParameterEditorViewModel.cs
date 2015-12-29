using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class PapyrusParameterEditorViewModel : PapyrusVariableParameterEditorViewModel
    {
        private PapyrusParameterDefinition parameter;

        public PapyrusParameterEditorViewModel(IEnumerable<string> types, PapyrusParameterDefinition parameter = null) : base(types)
        {
            this.parameter = parameter;

            if (this.parameter != null)
            {
                Name = this.parameter.Name.Value;
                SelectedType = TypeReferences.FirstOrDefault(t => t.ToString().ToLower() == this.parameter.TypeName.Value.ToLower());
                if (SelectedType == null)
                    SelectedType = this.parameter.TypeName.Value.ToLower();
            }
        }
    }
}