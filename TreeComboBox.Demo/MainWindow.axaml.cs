using System;
using Avalonia.Controls;

namespace TreeComboBox.Demo
{
    public partial class MainWindow : Window
    {
        private TreeViewVm Vm => (TreeViewVm)DataContext;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new TreeViewVm();
        }

        private void TreeComboBox_OnTextChanged(object? sender, TextChangedEventArgs e)
        {
            var txt = (e.Source as TextBox)?.Text;
            if (txt == null) return;
            Vm.Filter(txt);
        }

        private void TreeView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("TreeView_OnSelectionChanged：");
        }
    }
}