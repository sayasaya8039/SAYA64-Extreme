using System.Windows;
using System.Windows.Controls;
using SAYA64Extreme.Models;
using SAYA64Extreme.ViewModels;

namespace SAYA64Extreme;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.Initialize();
        }
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.Cleanup();
        }
    }

    private void CategoryTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainViewModel vm && e.NewValue is CategoryNode node)
        {
            vm.OnCategorySelected(node);
        }
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e) => Close();

    private void ExpandAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in CategoryTree.Items)
        {
            if (CategoryTree.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem tvi)
                SetExpanded(tvi, true);
        }
    }

    private void CollapseAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in CategoryTree.Items)
        {
            if (CategoryTree.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem tvi)
                SetExpanded(tvi, false);
        }
    }

    private void SetExpanded(TreeViewItem item, bool expanded)
    {
        item.IsExpanded = expanded;
        foreach (var child in item.Items)
        {
            if (item.ItemContainerGenerator.ContainerFromItem(child) is TreeViewItem childItem)
                SetExpanded(childItem, expanded);
        }
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "SAYA64 Extreme v2.0.0\n\nSystem Information & Diagnostic Tool\n\nAIDA64-inspired system information utility.\nPowered by LibreHardwareMonitor.",
            "About SAYA64 Extreme",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
