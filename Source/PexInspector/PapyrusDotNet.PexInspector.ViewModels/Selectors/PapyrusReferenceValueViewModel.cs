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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PapyrusAssembly.Extensions;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;

#endregion

namespace PapyrusDotNet.PexInspector.ViewModels.Selectors
{
    public class PapyrusReferenceValueViewModel : ViewModelBase
    {
        public static PapyrusReferenceValueViewModel DesignInstance = designInstance ??
                                                                      (designInstance =
                                                                          new PapyrusReferenceValueViewModel(null, null,
                                                                              null, null));

        private static PapyrusReferenceValueViewModel designInstance;
        private readonly PapyrusMethodDefinition currentMethod;
        private readonly PapyrusTypeDefinition currentType;
        private readonly OpCodeArgumentDescription desc;
        private readonly List<PapyrusAssemblyDefinition> loadedAssemblies;

        private ObservableCollection<FrameworkElement> comboBoxItems;
        private ObservableCollection<PapyrusMemberReference> referenceCollection;
        private Visibility referenceSelectionVisible;
        private PapyrusMemberReference selectedReference;
        private ComboBoxItem selectedReferenceType;

        public PapyrusReferenceValueViewModel(List<PapyrusAssemblyDefinition> loadedAssemblies,
            PapyrusTypeDefinition currentType, PapyrusMethodDefinition currentMethod, OpCodeArgumentDescription desc)
        {
            this.loadedAssemblies = loadedAssemblies;
            this.currentType = currentType;
            this.currentMethod = currentMethod;
            this.desc = desc;
            if (currentMethod != null)
            {
                ReferenceCollection = new ObservableCollection<PapyrusMemberReference>(currentMethod.GetVariables());
            }
            ComboBoxItems = new ObservableCollection<FrameworkElement>(CreateComboBoxItems());
            SelectedReferenceType = ComboBoxItems.First() as ComboBoxItem;
            ReferenceSelectionVisible = Visibility.Visible;
        }

        public ObservableCollection<FrameworkElement> ComboBoxItems
        {
            get { return comboBoxItems; }
            set { Set(ref comboBoxItems, value); }
        }

        public PapyrusMemberReference SelectedReference
        {
            get { return selectedReference; }
            set { Set(ref selectedReference, value); }
        }

        public ComboBoxItem SelectedReferenceType
        {
            get { return selectedReferenceType; }
            set
            {
                if (Set(ref selectedReferenceType, value))
                {
                    UpdateReferenceCollection(value);
                }
            }
        }


        public ObservableCollection<PapyrusMemberReference> ReferenceCollection
        {
            get { return referenceCollection; }
            set { Set(ref referenceCollection, value); }
        }

        public string SelectedReferenceName { get; set; }

        public Visibility ReferenceSelectionVisible
        {
            get { return referenceSelectionVisible; }
            set { Set(ref referenceSelectionVisible, value); }
        }

        private List<FrameworkElement> CreateComboBoxItems()
        {
            var elements = new List<FrameworkElement>();
            elements.Add(new ComboBoxItem { Content = "Variable" });
            elements.Add(new ComboBoxItem { Content = "Parameter" });
            elements.Add(new ComboBoxItem { Content = "Field" });
            //if (desc == null || desc.Constraints.Length == 0)
            {
                elements.Add(new ComboBoxItem { Content = "Self" });
                elements.Add(new ComboBoxItem { Content = "SelfRef" });
            }
            return elements;
        }

        private void UpdateReferenceCollection(ComboBoxItem value)
        {
            if (currentMethod == null) return;
            var tar = value.Content.ToString().ToLower();
            ReferenceSelectionVisible = Visibility.Visible;
            if (tar == "variable")
            {
                var papyrusVariableReferences = currentMethod.GetVariables();
                papyrusVariableReferences.ForEach(i => i.Type = PapyrusPrimitiveType.Reference);
                ReferenceCollection = new ObservableCollection<PapyrusMemberReference>(Filter(papyrusVariableReferences));
            }
            else if (tar == "parameter")
            {
                var papyrusParameterDefinitions = currentMethod.Parameters;
                ReferenceCollection =
                    new ObservableCollection<PapyrusMemberReference>(Filter(papyrusParameterDefinitions));
            }
            else if (tar == "field")
            {
                var papyrusFieldDefinitions = currentType.Fields.ToList();
                ReferenceCollection = new ObservableCollection<PapyrusMemberReference>(Filter(papyrusFieldDefinitions));
            }
            else if (tar == "none")
            {
                if (currentType.Assembly != null)
                {
                    SelectedReference = new PapyrusVariableReference
                    {
                        Name = "".Ref(currentType.Assembly),
                        Value = null,
                        Type = PapyrusPrimitiveType.None
                    };
                }
                ReferenceSelectionVisible = Visibility.Collapsed;
            }
            else if (tar == "self")
            {
                if (currentType.Assembly != null)
                {
                    SelectedReference = new PapyrusVariableReference
                    {
                        Name= "Self".Ref(currentType.Assembly),
                        Value = "Self",
                        Type = PapyrusPrimitiveType.Reference
                    };
                }
                ReferenceSelectionVisible = Visibility.Collapsed;
            }
            else if (tar == "selfref")
            {
                if (currentType.Assembly != null)
                {
                    SelectedReference = new PapyrusVariableReference
                    {
                        Name = "SelfRef".Ref(currentType.Assembly),
                        Value = "SelfRef",
                        Type = PapyrusPrimitiveType.Reference
                    };
                }
                ReferenceSelectionVisible = Visibility.Collapsed;
            }
        }


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
    }
}