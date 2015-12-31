using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using PapyrusDotNet.Common.Utilities;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Extensions;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;

namespace PapyrusDotNet.PexInspector.ViewModels.Selectors
{
    public class PapyrusReferenceAndConstantValueViewModel : ViewModelBase
    {
        private readonly List<PapyrusAssemblyDefinition> loadedAssemblies;
        private readonly PapyrusTypeDefinition currentType;
        private readonly PapyrusMethodDefinition currentMethod;
        private readonly OpCodeArgumentDescription desc;
        private readonly PapyrusPrimitiveType[] argumentTypes;

        public PapyrusReferenceAndConstantValueViewModel(List<PapyrusAssemblyDefinition> loadedAssemblies, PapyrusTypeDefinition currentType,
            PapyrusMethodDefinition currentMethod, OpCodeArgumentDescription desc, PapyrusPrimitiveType[] argumentTypes)
        {
            this.loadedAssemblies = loadedAssemblies;
            this.currentType = currentType;
            this.currentMethod = currentMethod;
            this.desc = desc;
            this.argumentTypes = argumentTypes;

            if (argumentTypes == null)
                argumentTypes = new PapyrusPrimitiveType[0];

            // Update the type constraints so we are only able to assign
            // values of the correct types.
            if (desc != null && argumentTypes.Length > 0)
            {
                UpdateTypeConstraints(argumentTypes);
            }

            if (currentMethod != null)
            {
                var references = new List<PapyrusMemberReference>();
                references.AddRange(currentMethod.Parameters);
                references.AddRange(currentMethod.GetVariables());
                if (currentType != null)
                    references.AddRange(currentType.Fields);

                ReferenceCollection = new ObservableCollection<PapyrusMemberReference>(references);

                HideValueInputs();

                ReferenceValueVisibility = Visibility.Visible;

                //SelectedValueType = ReferenceCollection.LastOrDefault();
            }
            ComboBoxItems = new ObservableCollection<FrameworkElement>(CreateComboBoxItems());
            SelectedValueType = ComboBoxItems.First() as ComboBoxItem;
        }

        private void UpdateTypeConstraints(PapyrusPrimitiveType[] argumentTypes)
        {
            // If the constraint is a reference, then we must do a lookup and find
            // the original type of that reference.... Gah.. Not today

            var items = this.desc.Constraints.ToList();
            if (this.desc.Constraints.Contains(OpCodeConstraint.Value0))
            {
                var c = GetConstraint(argumentTypes[0]);
                if (c != OpCodeConstraint.NoConstraints)
                {
                    items.Remove(OpCodeConstraint.Value0);
                    items.Add(c);
                }
            }
            if (this.desc.Constraints.Contains(OpCodeConstraint.Value1))
            {
                var c = GetConstraint(argumentTypes[1]);
                if (c != OpCodeConstraint.NoConstraints)
                {
                    items.Remove(OpCodeConstraint.Value1);
                    items.Add(c);
                }
            }
            if (this.desc.Constraints.Contains(OpCodeConstraint.Value2))
            {
                var c = GetConstraint(argumentTypes[2]);
                if (c != OpCodeConstraint.NoConstraints)
                {
                    items.Remove(OpCodeConstraint.Value2);
                    items.Add(c);
                }
            }
            if (this.desc.Constraints.Contains(OpCodeConstraint.Value3))
            {
                var c = GetConstraint(argumentTypes[3]);
                if (c != OpCodeConstraint.NoConstraints)
                {
                    items.Remove(OpCodeConstraint.Value3);
                    items.Add(c);
                }
            }
            if (this.desc.Constraints.Contains(OpCodeConstraint.Value4))
            {
                var c = GetConstraint(argumentTypes[4]);
                if (c != OpCodeConstraint.NoConstraints)
                {
                    items.Remove(OpCodeConstraint.Value4);
                    items.Add(c);
                }
            }
            this.desc.Constraints = items.ToArray();
        }

        private OpCodeConstraint GetConstraint(PapyrusPrimitiveType argumentType)
        {
            switch (argumentType)
            {
                case PapyrusPrimitiveType.Boolean: return OpCodeConstraint.Boolean;
                case PapyrusPrimitiveType.String: return OpCodeConstraint.String;
                case PapyrusPrimitiveType.Integer: return OpCodeConstraint.Integer;
                case PapyrusPrimitiveType.Float: return OpCodeConstraint.Float;
                case PapyrusPrimitiveType.None: return OpCodeConstraint.None;
                default:
                    return OpCodeConstraint.NoConstraints;
            }
        }

        private ObservableCollection<FrameworkElement> comboBoxItems;
        public ObservableCollection<FrameworkElement> ComboBoxItems
        {
            get { return comboBoxItems; }
            set { Set(ref comboBoxItems, value); }
        }

        private List<FrameworkElement> CreateComboBoxItems()
        {
            var elements = new List<FrameworkElement>();

            if (desc != null && desc.Constraints.Length > 0)
            {
                if (desc.Constraints.Contains(OpCodeConstraint.None))
                    elements.Add(new ComboBoxItem { Content = "None" });
                if (desc.Constraints.Contains(OpCodeConstraint.Integer))
                    elements.Add(new ComboBoxItem { Content = "Integer" });
                if (desc.Constraints.Contains(OpCodeConstraint.Float))
                    elements.Add(new ComboBoxItem { Content = "Float" });
                if (desc.Constraints.Contains(OpCodeConstraint.Boolean))
                    elements.Add(new ComboBoxItem { Content = "Boolean" });
                if (desc.Constraints.Contains(OpCodeConstraint.String))
                    elements.Add(new ComboBoxItem { Content = "String" });

                elements.Add(new Separator());
                elements.Add(new ComboBoxItem { Content = "Parameter" });
                elements.Add(new ComboBoxItem { Content = "Variable" });
                elements.Add(new ComboBoxItem { Content = "Field" });

                // if(desc.)
                elements.Add(new ComboBoxItem { Content = "Self" });
                elements.Add(new ComboBoxItem { Content = "SelfRef" });
                return elements;
            }


            elements.Add(new ComboBoxItem { Content = "None" });
            elements.Add(new ComboBoxItem { Content = "Integer" });
            elements.Add(new ComboBoxItem { Content = "Float" });
            elements.Add(new ComboBoxItem { Content = "Boolean" });
            elements.Add(new ComboBoxItem { Content = "String" });
            elements.Add(new Separator());
            elements.Add(new ComboBoxItem { Content = "Parameter" });
            elements.Add(new ComboBoxItem { Content = "Variable" });
            elements.Add(new ComboBoxItem { Content = "Field" });

            elements.Add(new ComboBoxItem { Content = "Self" });
            elements.Add(new ComboBoxItem { Content = "SelfRef" });
            return elements;
        }

        //elements.Add(new Separator());

        private void HideValueInputs()
        {
            ConstantValueVisibility = Visibility.Collapsed;
            ReferenceValueVisibility = Visibility.Collapsed;
        }

        public object SelectedItem
        {
            get { return selectedItem; }
            set { Set(ref selectedItem, value); }
        }

        public Visibility ConstantValueVisibility
        {
            get { return constantValueVisibility; }
            set { Set(ref constantValueVisibility, value); }
        }

        public object SelectedConstantValue
        {
            get { return selectedConstantValue; }
            set
            {
                if (Set(ref selectedConstantValue, value))
                {
                    SelectedItem = value;
                    UpdateSelectedValue();
                    SelectedReferenceValue = null;
                }
            }
        }

        private void UpdateSelectedValue()
        {
            var val = selectedValueType as ComboBoxItem;
            var tar = val.Content.ToString().ToLower();
            if (tar == "variable" || tar == "parameter" || tar == "field" || tar == "self" || tar == "selfref")
                return;
            if (tar == "string")
            {
                if (currentType.Assembly != null)
                {
                    SelectedItem = new PapyrusVariableReference
                    {
                        Value = selectedConstantValue.ToString().Ref(currentType.Assembly).Value,
                        ValueType = PapyrusPrimitiveType.String
                    };
                }
            }
            else
            {
                var type = Utility.GetPapyrusPrimitiveType(Utility.GetPapyrusReturnType(tar));
                if (currentType.Assembly != null)
                {
                    SelectedItem = new PapyrusVariableReference
                    {
                        Value = selectedConstantValue,
                        ValueType = type
                    };
                }
            }

        }

        public object SelectedValueType
        {
            get { return selectedValueType; }
            set
            {
                if (Set(ref selectedValueType, value))
                {
                    var val = value as ComboBoxItem;
                    var tar = val.Content.ToString().ToLower();
                    SelectedTypeName = tar;
                    var isRef = false;
                    if (tar == "variable")
                    {
                        var papyrusVariableReferences = Filter(currentMethod.GetVariables());

                        ReferenceCollection = new ObservableCollection<PapyrusMemberReference>(papyrusVariableReferences);
                        isRef = true;
                    }
                    else if (tar == "parameter")
                    {
                        var papyrusParameterDefinitions = Filter(currentMethod.Parameters);
                        ReferenceCollection =
                            new ObservableCollection<PapyrusMemberReference>(papyrusParameterDefinitions);
                        isRef = true;
                    }
                    else if (tar == "field")
                    {
                        var papyrusFieldDefinitions = Filter(currentType.Fields.ToList());
                        ReferenceCollection = new ObservableCollection<PapyrusMemberReference>(papyrusFieldDefinitions);
                        isRef = true;
                    }
                    else if (tar == "self")
                    {
                        ConstantValueVisibility = Visibility.Collapsed;
                        SelectedConstantValue = null;
                        isRef = true;
                        if (currentType.Assembly != null)
                        {
                            SelectedItem = new PapyrusVariableReference
                            {
                                Value = "self".Ref(currentType.Assembly).Value,
                                ValueType = PapyrusPrimitiveType.Reference
                            };
                        }
                        else
                            SelectedItem = "Self";
                    }
                    else if (tar == "selfref")
                    {
                        ConstantValueVisibility = Visibility.Collapsed;
                        SelectedConstantValue = null;
                        isRef = true;
                        if (currentType.Assembly != null)
                        {
                            SelectedItem = new PapyrusVariableReference
                            {
                                Value = "SelfRef".Ref(currentType.Assembly).Value,
                                ValueType = PapyrusPrimitiveType.Reference
                            };
                        }
                        else
                            SelectedItem = "SelfRef";
                    }
                    else if (tar == "none")
                    {
                        ConstantValueVisibility = Visibility.Collapsed;
                        SelectedConstantValue = null;
                        SelectedItem = null;
                    }


                    if (isRef)
                    {
                        ReferenceValueVisibility = Visibility.Visible;
                        ConstantValueVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        ReferenceValueVisibility = Visibility.Collapsed;
                        ConstantValueVisibility = Visibility.Visible;
                    }
                }
            }
        }

        public string SelectedTypeName { get; set; }


        private IEnumerable<PapyrusMemberReference> Filter(List<PapyrusParameterDefinition> collection)
        {
            if (desc == null || desc.Constraints.Length == 0) return collection;
            var result = new List<PapyrusMemberReference>();
            foreach (var constraint in desc.Constraints)
            {
                var type = constraint.ToString().ToLower();
                if (type == "boolean") type = "bool";
                if (type == "integer") type = "int";
                var range = collection.Where(i => i.TypeName.Value.ToLower() == type);
                result.AddRange(range);
            }
            return result;
        }


        private IEnumerable<PapyrusMemberReference> Filter(List<PapyrusFieldDefinition> collection)
        {
            if (desc == null || desc.Constraints.Length == 0) return collection;
            var result = new List<PapyrusMemberReference>();
            foreach (var constraint in desc.Constraints)
            {
                var type = constraint.ToString().ToLower();
                if (type == "boolean") type = "bool";
                if (type == "integer") type = "int";
                var range = collection.Where(i => i.TypeName.ToLower() == type);
                result.AddRange(range);
            }
            return result;
        }

        private IEnumerable<PapyrusMemberReference> Filter(List<PapyrusVariableReference> collection)
        {
            if (desc == null || desc.Constraints.Length == 0) return collection;
            var result = new List<PapyrusMemberReference>();
            foreach (var constraint in desc.Constraints)
            {
                var type = constraint.ToString().ToLower();
                if (type == "boolean") type = "bool";
                if (type == "integer") type = "int";
                var range = collection.Where(i => i.TypeName.Value.ToLower() == type);
                result.AddRange(range);
            }
            return result;
        }

        public Visibility ReferenceValueVisibility
        {
            get { return referenceValueVisibility; }
            set { Set(ref referenceValueVisibility, value); }
        }


        public object SelectedReferenceValue
        {
            get { return selectedReferenceValue; }
            set
            {
                if (Set(ref selectedReferenceValue, value))
                {
                    SelectedConstantValue = null;
                    SelectedItem = value;
                }
            }
        }

        public ObservableCollection<PapyrusMemberReference> ReferenceCollection
        {
            get { return referenceCollection; }
            set { Set(ref referenceCollection, value); }
        }

        public string SelectedReferenceName { get; set; }

        private static Lazy<PapyrusReferenceAndConstantValueViewModel> lazyDesignInstance =
            new Lazy<PapyrusReferenceAndConstantValueViewModel>(CreateDesignViewModel);

        private static PapyrusReferenceAndConstantValueViewModel CreateDesignViewModel() => new PapyrusReferenceAndConstantValueViewModel(null, null, null, null, null);

        public static PapyrusReferenceAndConstantValueViewModel DesignInstance = lazyDesignInstance.Value;



        private object selectedItem;
        private Visibility constantValueVisibility;
        private object selectedConstantValue;
        private object selectedValueType;
        private Visibility referenceValueVisibility;
        private object selectedReferenceValue;
        private ObservableCollection<PapyrusMemberReference> referenceCollection;
    }
}