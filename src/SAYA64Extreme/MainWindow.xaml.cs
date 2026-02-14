using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using SAYA64Extreme.Models;
using SAYA64Extreme.Services;
using SAYA64Extreme.ViewModels;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Forms = System.Windows.Forms;

namespace SAYA64Extreme;

public partial class MainWindow : Window
{
    private Forms.NotifyIcon? _notifyIcon;
    private bool _isExiting;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        InitializeSystemTray();
    }

    private void InitializeSystemTray()
    {
        _notifyIcon = new Forms.NotifyIcon
        {
            Text = "SAYA64 Extreme",
            Visible = true
        };

        // Load icon from embedded resource
        var iconUri = new Uri("pack://application:,,,/Resources/app.ico", UriKind.Absolute);
        try
        {
            using var stream = Application.GetResourceStream(iconUri)?.Stream;
            if (stream != null)
                _notifyIcon.Icon = new Icon(stream);
        }
        catch
        {
            _notifyIcon.Icon = SystemIcons.Application;
        }

        // Context menu
        var contextMenu = new Forms.ContextMenuStrip();
        var showItem = new Forms.ToolStripMenuItem("表示 / Show");
        showItem.Click += (_, _) => RestoreFromTray();
        var separator = new Forms.ToolStripSeparator();
        var exitItem = new Forms.ToolStripMenuItem("終了 / Exit");
        exitItem.Click += (_, _) =>
        {
            _isExiting = true;
            Close();
        };

        contextMenu.Items.Add(showItem);
        contextMenu.Items.Add(separator);
        contextMenu.Items.Add(exitItem);
        _notifyIcon.ContextMenuStrip = contextMenu;

        // Double-click to restore
        _notifyIcon.DoubleClick += (_, _) => RestoreFromTray();
    }

    private void RestoreFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
            _notifyIcon?.ShowBalloonTip(1000, "SAYA64 Extreme",
                LanguageService.GetString("Status_SensorActive"),
                Forms.ToolTipIcon.Info);
        }
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
        if (!_isExiting)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            return;
        }

        if (DataContext is MainViewModel vm)
        {
            vm.Cleanup();
        }

        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }
    }

    private void CategoryTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainViewModel vm && e.NewValue is CategoryNode node)
        {
            vm.OnCategorySelected(node);
        }
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _isExiting = true;
        Close();
    }

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
