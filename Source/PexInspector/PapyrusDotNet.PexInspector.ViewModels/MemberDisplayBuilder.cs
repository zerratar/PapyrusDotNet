using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class MemberDisplayBuilder : IMemberDisplayBuilder
    {
        private static readonly SolidColorBrush AttributeColor = new SolidColorBrush(Color.FromRgb(30, 78, 135));
        private static readonly SolidColorBrush TypeColor = new SolidColorBrush(Color.FromRgb(30, 135, 75));
        private static readonly SolidColorBrush MethodColor = new SolidColorBrush(Color.FromRgb(44, 62, 80));

        public ObservableCollection<Inline> BuildMemberDisplay(object item)
        {
            var displayItems = new List<Run>();
            var type = item as PapyrusTypeDefinition;
            if (type != null)
            {
                displayItems.Add(new Run(type.Name.Value) { Foreground = MethodColor, FontWeight = FontWeights.DemiBold });
                if (type.BaseTypeName != null && !string.IsNullOrEmpty(type.BaseTypeName.Value))
                {
                    displayItems.Add(new Run(" : "));
                    displayItems.Add(new Run(type.BaseTypeName.Value) { Foreground = TypeColor });
                }
            }
            var state = item as PapyrusStateDefinition;
            if (state != null)
            {
                displayItems.Add(new Run(string.IsNullOrEmpty(state.Name.Value) ? "<Default>" : state.Name.Value));
                displayItems.Add(new Run(" State"));
            }
            var assembly = item as PapyrusAssemblyDefinition;
            if (assembly != null)
            {
                displayItems.Add(new Run(assembly.Types.First().Name.Value + ".pex"));
            }
            var prop = item as PapyrusPropertyDefinition;
            if (prop != null)
            {
                if (prop.IsAuto)
                {
                    displayItems.Add(new Run("Auto") { Foreground = AttributeColor });
                    displayItems.Add(new Run(" "));
                }

                displayItems.Add(new Run("Property") { Foreground = AttributeColor });
                displayItems.Add(new Run(" "));

                displayItems.Add(new Run(prop.TypeName.Value) { Foreground = TypeColor });
                displayItems.Add(new Run(" "));
                displayItems.Add(new Run(prop.Name.Value));
            }
            var field = item as PapyrusFieldDefinition;
            if (field != null)
            {
                displayItems.Add(new Run(field.TypeName) { Foreground = TypeColor });
                displayItems.Add(new Run(" "));
                displayItems.Add(new Run(field.Name.Value));
            }
            var method = item as PapyrusMethodDefinition;
            if (method != null)
            {
                if (method.IsNative)
                {
                    displayItems.Add(new Run("Native") { Foreground = AttributeColor });
                    displayItems.Add(new Run(" "));
                }
                if (method.IsGlobal)
                {
                    displayItems.Add(new Run("Global") { Foreground = AttributeColor });
                    displayItems.Add(new Run(" "));
                }

                if (method.IsEvent)
                {
                    displayItems.Add(new Run("Event") { Foreground = AttributeColor });
                    displayItems.Add(new Run(" "));
                }
                else
                {
                    displayItems.Add(new Run(method.ReturnTypeName.Value) { Foreground = TypeColor });
                    displayItems.Add(new Run(" "));
                }
                var nameRef = method.Name;
                var name = nameRef?.Value ??
                           (method.IsSetter
                               ? method.PropName + ".Setter"
                               : method.IsGetter ? method.PropName + ".Getter" : "?????");
                displayItems.Add(new Run(name) { Foreground = MethodColor, FontWeight = FontWeights.DemiBold });
                displayItems.AddRange(GetParameterRuns(method.Parameters));
                displayItems.Add(new Run(";"));
            }
            return new ObservableCollection<Inline>(displayItems.ToArray());
        }


        public List<Run> GetParameterRuns(List<PapyrusParameterDefinition> parameters)
        {
            var output = new List<Run>();
            output.Add(new Run("("));
            for (var index = 0; index < parameters.Count; index++)
            {
                var p = parameters[index];
                output.Add(new Run(p.TypeName.Value) { Foreground = TypeColor });
                output.Add(new Run(" "));
                output.Add(new Run(p.Name.Value));

                if (index != parameters.Count - 1)
                {
                    output.Add(new Run(", "));
                }
            }

            output.Add(new Run(")"));
            return output;
        }
    }
}