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
using GalaSoft.MvvmLight;
using PapyrusDotNet.PapyrusAssembly;
using PapyrusDotNet.PexInspector.ViewModels.Implementations;

#endregion

namespace PapyrusDotNet.PexInspector.ViewModels.Selectors
{
    public class PapyrusInstructionSelectorViewModel : ViewModelBase
    {
        public static PapyrusInstructionSelectorViewModel DesignInstance = designInstance ??
                                                                           (designInstance =
                                                                               new PapyrusInstructionSelectorViewModel(
                                                                                   null, null));

        private static PapyrusInstructionSelectorViewModel designInstance;
        private ObservableCollection<PapyrusInstruction> instructions;
        private PapyrusInstruction selectedInstruction;

        public PapyrusInstructionSelectorViewModel(IEnumerable<PapyrusInstruction> instructions,
            OpCodeArgumentDescription opCodeArgumentDescription)
        {
            if (instructions != null)
            {
                Instructions = new ObservableCollection<PapyrusInstruction>(instructions);
            }
        }

        public ObservableCollection<PapyrusInstruction> Instructions
        {
            get { return instructions; }
            set { Set(ref instructions, value); }
        }

        public PapyrusInstruction SelectedInstruction
        {
            get { return selectedInstruction; }
            set { Set(ref selectedInstruction, value); }
        }
    }
}