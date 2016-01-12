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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;

#endregion

namespace PapyrusDotNet.PexInspector.ViewModels.Selectors
{
    public class PapyrusMethodSelectorViewModel : ViewModelBase
    {
        public static PapyrusMethodSelectorViewModel DesignInstance = designInstance ??
                                                                      (designInstance =
                                                                          new PapyrusMethodSelectorViewModel(null, null,
                                                                              null));

        private static PapyrusMethodSelectorViewModel designInstance;
        private readonly List<PapyrusAssemblyDefinition> loadedAssemblies;
        private ObservableCollection<PapyrusViewModel> methods;
        private PapyrusViewModel selectedMethod;
        private string selectedMethodName;

        public PapyrusMethodSelectorViewModel(List<PapyrusAssemblyDefinition> loadedAssemblies,
            PapyrusTypeDefinition currentType, OpCodeArgumentDescription opCodeArgumentDescription)
        {
            this.loadedAssemblies = loadedAssemblies;

            if (currentType != null)
            {
                Methods =
                    new ObservableCollection<PapyrusViewModel>(
                        currentType.States.SelectMany(s => s.Methods)
                            .OrderBy(m => m.Name?.Value)
                            .Select(j => new PapyrusViewModel
                            {
                                Text = j.Name.Value + GetParameterString(j.Parameters) + " : " + j.ReturnTypeName.Value,
                                Item = j
                            }));
            }

            SelectedMethodCommand = new RelayCommand<PapyrusViewModel>(SelectMethod);
        }

        public RelayCommand<PapyrusViewModel> SelectedMethodCommand { get; set; }

        public PapyrusViewModel SelectedMethod
        {
            get { return selectedMethod; }
            set
            {
                if (Set(ref selectedMethod, value))
                {
                    if (value != null)
                    {
                        var method = value.Item as PapyrusMethodDefinition;
                        if (method != null)
                            SelectedMethodName = method.Name.Value;
                    }
                }
            }
        }

        public ObservableCollection<PapyrusViewModel> Methods
        {
            get { return methods; }
            set { Set(ref methods, value); }
        }

        public string SelectedMethodName
        {
            get { return selectedMethodName; }
            set { Set(ref selectedMethodName, value); }
        }

        private void SelectMethod(PapyrusViewModel obj)
        {
            SelectedMethod = obj;
        }

        private string GetParameterString(List<PapyrusParameterDefinition> parameters,
            bool includeParameterNames = false)
        {
            var paramDefs = string.Join(", ", parameters.Select(p => p.TypeName.Value +
                                                                     (includeParameterNames ? " " + p.Name.Value : "")));

            return "(" + paramDefs + ")";
        }
    }
}