using System.Windows;
using System.Windows.Controls;
using SAYA64Extreme.Models;
using SAYA64Extreme.Services;
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

    private void Language_Japanese_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.ChangeLanguage("ja");
    }

    private void Language_English_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.ChangeLanguage("en");
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            LanguageService.GetString("About_Message"),
            LanguageService.GetString("About_Title"),
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
