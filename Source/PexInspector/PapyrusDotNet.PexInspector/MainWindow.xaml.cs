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

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using PapyrusDotNet.PapyrusAssembly.Extensions;
using PapyrusDotNet.PexInspector.Implementations;
using PapyrusDotNet.PexInspector.ViewModels;

#endregion

namespace PapyrusDotNet.PexInspector
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double lastHorizontalOffset;

        private bool userInvokedBringIntoView = false;
        private readonly MainWindowViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            viewModel = new MainWindowViewModel(new DialogService(), App.OpenFile);
            viewModel.SelectedContentIndexChanged +=
                (sender, args) => { AnchorablePane.SelectedContentIndex = viewModel.SelectedContentIndex; };
            DataContext = viewModel;

            SetSyntaxHighlighting();
        }

        private void SetSyntaxHighlighting()
        {
            IHighlightingDefinition customHighlighting;
            using (Stream s = File.OpenRead("Assets/Papyrus-Mode.xshd"))
                //typeof(MainWindow).Assembly.GetManifestResourceStream("AvalonEdit.Sample.CustomHighlighting.xshd"))
            {
                using (XmlReader reader = new XmlTextReader(s))
                {
                    customHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            // and register it in the HighlightingManager
            HighlightingManager.Instance.RegisterHighlighting("Papyrus", new[] {".psc"}, customHighlighting);
            textEditor.SyntaxHighlighting = customHighlighting;
        }

        private void TreeViewItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            //if (userInvokedBringIntoView)
            //{
            //    var scrollViewer = FindChild<ScrollViewer>(treeView);
            //    if (e.TargetRect.Left > 0)
            //    {
            //        scrollViewer.ScrollToHorizontalOffset(lastHorizontalOffset);
            //        scrollViewer.ScrollToVerticalOffset(e.TargetRect.Top);
            //    }
            //    userInvokedBringIntoView = false;
            //    e.Handled = true;
            //}
        }

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //var scrollViewer = FindChild<ScrollViewer>(treeView);

            //userInvokedBringIntoView = true;
            //lastHorizontalOffset = scrollViewer.HorizontalOffset;
        }

        public static T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            // Confirm parent is valid.
            if (parent == null) return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                var childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else
                {
                    // child element found.
                    foundChild = (T) child;
                    break;
                }
            }
            return foundChild;
        }

        public static T FindChild<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            // Confirm parent and childName are valid.
            if (parent == null) return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                var childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T) child;
                        break;
                    }
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else
                {
                    // child element found.
                    foundChild = (T) child;
                    break;
                }
            }

            return foundChild;
        }

        public static T FindAncestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            current = VisualTreeHelper.GetParent(current);

            while (current != null)
            {
                if (current is T)
                {
                    return (T) current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private void RowContextMenu_OnOpened(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            if (vm != null)
            {
                vm.RaiseCommandsCanExecute();
            }
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            var toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }

            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness(0);
            }
        }

        private void TreeView_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                files.ForEach(viewModel.LoadPex);
            }
        }
    }
}