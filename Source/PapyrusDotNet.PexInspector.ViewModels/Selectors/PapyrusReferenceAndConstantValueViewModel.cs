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
        private readonly OpCodeArgumentDescription opCodeArgumentDescription;

        public PapyrusReferenceAndConstantValueViewModel(List<PapyrusAssemblyDefinition> loadedAssemblies, PapyrusTypeDefinition currentType,
            PapyrusMethodDefinition currentMethod, OpCodeArgumentDescription opCodeArgumentDescription)
        {
            this.loadedAssemblies = loadedAssemblies;
            this.currentType = currentType;
            this.currentMethod = currentMethod;
            this.opCodeArgumentDescription = opCodeArgumentDescription;

            if (currentMethod != null)
            {
                var references = new List<PapyrusMemberReference>();
                references.AddRange(currentMethod.Parameters);
                references.AddRange(currentMethod.GetVariables());
                references.AddRange(currentType.Fields);

                ReferenceCollection = new ObservableCollection<PapyrusMemberReference>(references);

                HideValueInputs();

                ReferenceValueVisibility = Visibility.Visible;

                //SelectedValueType = ReferenceCollection.LastOrDefault();
            }
        }

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
                    if (val.Content.ToString().ToLower().Contains("reference"))
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