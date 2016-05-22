using PapyrusDotNet.Common;
using PapyrusDotNet.PexInspector.Implementations;
using PapyrusDotNet.PexInspector.ViewModels;
using PapyrusDotNet.PexInspector.ViewModels.Interfaces;

namespace PapyrusDotNet.PexInspector
{
    public class DependencyLocator
    {
        private IoCContainer ioc;

        public DependencyLocator()
        {
            ioc = new IoCContainer()
                .Register<IDialogService, DialogService>()
                .Register<IPexLoader, PexLoader>()
                .Register<IPexTreeBuilder, PexTreeBuilder>()
                .Register<IMemberDisplayBuilder, MemberDisplayBuilder>()
                .Register<MainWindowViewModel, MainWindowViewModel>();
        }

        public T Resolve<T>()
        {
            return ioc.Resolve<T>();
        }
    }
}