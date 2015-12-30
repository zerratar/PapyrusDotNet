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
using PapyrusDotNet.PexInspector.ViewModels;

namespace PapyrusDotNet.PexInspector.Windows
{
    /// <summary>
    /// Interaction logic for PapyrusParameterAndVariableEditorWindow.xaml
    /// </summary>
    public partial class PapyrusParameterAndVariableEditorWindow : Window
    {
        public PapyrusParameterAndVariableEditorWindow()
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

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var vm = DataContext;
            var varEdit = vm as PapyrusVariableEditorViewModel;
            if (varEdit != null)
            {
                varEdit.SelectedTypeName = typeRef.Text;
            }
            var parEdit = vm as PapyrusParameterEditorViewModel;
            if (parEdit != null)
            {
                parEdit.SelectedTypeName = typeRef.Text;
            }
        }
    }
}
