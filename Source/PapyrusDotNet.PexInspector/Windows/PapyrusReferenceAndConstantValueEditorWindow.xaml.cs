using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PapyrusDotNet.PexInspector.ViewModels.Selectors;

namespace PapyrusDotNet.PexInspector.Windows
{
    /// <summary>
    /// Interaction logic for PapyrusParameterAndVariableEditorWindow.xaml
    /// </summary>
    public partial class PapyrusReferenceAndConstantValueEditorWindow : Window
    {
        public PapyrusReferenceAndConstantValueEditorWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            var vm = DataContext as PapyrusReferenceAndConstantValueViewModel;
            if (vm != null)
            {
                vm.SelectedReferenceName = cb.Text;
            }
        }
    }
}
