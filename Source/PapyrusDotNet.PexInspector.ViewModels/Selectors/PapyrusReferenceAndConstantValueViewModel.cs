using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;

namespace PapyrusDotNet.PexInspector.ViewModels.Selectors
{
    public class PapyrusReferenceAndConstantValueViewModel : ViewModelBase
    {
        private readonly List<PapyrusAssemblyDefinition> loadedAssemblies;
        private readonly PapyrusTypeDefinition currentType;
        private readonly PapyrusMethodDefinition currentMethod;
        private readonly OpCodeArgumentDescription desc;

        public PapyrusReferenceAndConstantValueViewModel(List<PapyrusAssemblyDefinition> loadedAssemblies, PapyrusTypeDefinition currentType,
            PapyrusMethodDefinition currentMethod, OpCodeArgumentDescription desc)
        {
            this.loadedAssemblies = loadedAssemblies;
            this.currentType = currentType;
            this.currentMethod = currentMethod;
            this.desc = desc;

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
                    SelectedReferenceValue = null;
                    SelectedItem = value;
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

                    if (tar == "self")
                    {
                        ConstantValueVisibility = Visibility.Collapsed;
                        SelectedConstantValue = null;

                        if (currentType.Assembly != null)
                        {
                            SelectedItem = new PapyrusVariableReference
                            {
                                Value = "self",
                                ValueType = PapyrusPrimitiveType.Reference
                            };
                        }
                        else
                            SelectedItem = "Self";
                    }
                    if (tar == "selfref")
                    {
                        ConstantValueVisibility = Visibility.Collapsed;
                        SelectedConstantValue = null;

                        if (currentType.Assembly != null)
                        {
                            SelectedItem = new PapyrusVariableReference
                            {
                                Value = "SelfRef",
                                ValueType = PapyrusPrimitiveType.Reference
                            };
                        }
                        else
                            SelectedItem = "SelfRef";
                    }
                    if (tar == "none")
                    {
                        ConstantValueVisibility = Visibility.Collapsed;
                        SelectedConstantValue = null;
                        SelectedItem = null;
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

        private static PapyrusReferenceAndConstantValueViewModel CreateDesignViewModel() => new PapyrusReferenceAndConstantValueViewModel(null, null, null, null);

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