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
    public class PapyrusTypeSelectorViewModel : ViewModelBase
    {
        public static PapyrusTypeSelectorViewModel DesignInstance = designInstance ??
                                                                    (designInstance =
                                                                        new PapyrusTypeSelectorViewModel(null, null));

        private static PapyrusTypeSelectorViewModel designInstance;
        private readonly List<PapyrusAssemblyDefinition> loadedAssemblies;
        private readonly OpCodeArgumentDescription opCodeArgumentDescription;
        private PapyrusTypeDefinition selectedType;
        private ObservableCollection<PapyrusViewModel> types;

        public PapyrusTypeSelectorViewModel(List<PapyrusAssemblyDefinition> loadedAssemblies,
            OpCodeArgumentDescription opCodeArgumentDescription)
        {
            this.loadedAssemblies = loadedAssemblies;
            this.opCodeArgumentDescription = opCodeArgumentDescription;

            SelectedTypeCommand = new RelayCommand<PapyrusViewModel>(SelectType);

            if (loadedAssemblies != null)
            {
                var defs = loadedAssemblies.SelectMany(t => t.Types).ToList();
                Types = new ObservableCollection<PapyrusViewModel>(
                    defs.Select(i => new PapyrusViewModel
                    {
                        Text =
                            i.Name.Value +
                            (!string.IsNullOrEmpty(i.BaseTypeName.Value) ? " : " + i.BaseTypeName.Value : ""),
                        Item = i
                    })
                    );
            }
        }

        public PapyrusTypeDefinition SelectedType
        {
            get { return selectedType; }
            set { Set(ref selectedType, value); }
        }

        public ObservableCollection<PapyrusViewModel> Types
        {
            get { return types; }
            set { Set(ref types, value); }
        }

        public RelayCommand<PapyrusViewModel> SelectedTypeCommand { get; set; }

        private void SelectType(PapyrusViewModel obj)
        {
            SelectedType = obj.Item as PapyrusTypeDefinition;
        }
    }
}