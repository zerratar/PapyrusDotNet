using System.Collections.Generic;
using System.Linq;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class PapyrusVariableEditorViewModel : PapyrusVariableParameterEditorViewModel
    {
        private PapyrusVariableReference variable;

        public PapyrusVariableEditorViewModel(IEnumerable<string> types, PapyrusVariableReference variable = null) : base(types)
        {
            this.variable = variable;

            if (this.variable != null)
            {
                Name = this.variable.Name.Value;
                SelectedType = TypeReferences.FirstOrDefault(t => t.ToString().ToLower() == this.variable.TypeName.Value.ToLower());
                if (SelectedType == null)
                    SelectedType = this.variable.TypeName.Value.ToLower();
            }
        }
    }
}