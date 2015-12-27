using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace PapyrusDotNet.PexInspector.ViewModels.Interfaces
{
    public interface IDialogService
    {
        IDialogResult ShowDialog(ViewModelBase viewModel);
    }
}
