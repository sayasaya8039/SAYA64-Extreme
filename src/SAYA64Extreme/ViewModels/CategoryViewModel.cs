using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SAYA64Extreme.Models;

namespace SAYA64Extreme.ViewModels;

/// <summary>
/// カテゴリツリーの構築・管理を担当するViewModel。
/// AIDA64風の左ペイン階層メニューを提供する。
/// </summary>
public partial class CategoryViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<CategoryNode> categories = new();

    [ObservableProperty]
    private CategoryNode? selectedCategory;

    [ObservableProperty]
    private string selectedCategoryName = "Computer";

    [ObservableProperty]
    private string selectedCategoryIcon = "💻";

    /// <summary>
    /// カテゴリ選択変更時のコールバック。MainViewModelから購読される。
    /// </summary>
    public event Action<CategoryNode>? CategorySelected;

    public void BuildCategoryTree()
    {
        Categories = new ObservableCollection<CategoryNode>
        {
            new CategoryNode
            {
                Id = "computer",
                Name = "Computer",
                Icon = "💻",
                Children = new ObservableCollection<CategoryNode>
                {
                    new() { Id = "summary", Name = "Summary", Icon = "📋" }
                }
            },
            new CategoryNode
            {
                Id = "motherboard",
                Name = "Motherboard",
                Icon = "🔧",
                Children = new ObservableCollection<CategoryNode>
                {
                    new() { Id = "cpu", Name = "CPU", Icon = "⚡" },
                    new() { Id = "memory", Name = "Memory", Icon = "💾" },
                    new() { Id = "spd", Name = "SPD", Icon = "📊" }
                }
            },
            new CategoryNode
            {
                Id = "display",
                Name = "Display",
                Icon = "🖥️",
                Children = new ObservableCollection<CategoryNode>
                {
                    new() { Id = "gpu", Name = "GPU", Icon = "🎮" },
                    new() { Id = "monitor", Name = "Monitor", Icon = "📺" }
                }
            },
            new CategoryNode
            {
                Id = "storage",
                Name = "Storage",
                Icon = "💿",
                Children = new ObservableCollection<CategoryNode>
                {
                    new() { Id = "disk", Name = "Disks", Icon = "🗄️" },
                    new() { Id = "smart", Name = "S.M.A.R.T.", Icon = "🔍" }
                }
            },
            new CategoryNode
            {
                Id = "network",
                Name = "Network",
                Icon = "🌐",
            },
            new CategoryNode
            {
                Id = "software",
                Name = "Software",
                Icon = "📦",
                Children = new ObservableCollection<CategoryNode>
                {
                    new() { Id = "programs", Name = "Installed Programs", Icon = "📋" },
                    new() { Id = "processes", Name = "Processes", Icon = "⚙️" }
                }
            },
            new CategoryNode
            {
                Id = "devices",
                Name = "Devices",
                Icon = "🔌",
                Children = new ObservableCollection<CategoryNode>
                {
                    new() { Id = "usb", Name = "USB Devices", Icon = "🔗" }
                }
            },
            new CategoryNode
            {
                Id = "os",
                Name = "Operating System",
                Icon = "🪟",
            },
            new CategoryNode
            {
                Id = "sensor",
                Name = "Sensor",
                Icon = "🌡️",
                Children = new ObservableCollection<CategoryNode>
                {
                    new() { Id = "sensor-temp", Name = "Temperatures", Icon = "🔴" },
                    new() { Id = "sensor-fan", Name = "Cooling Fans", Icon = "🌀" },
                    new() { Id = "sensor-voltage", Name = "Voltages", Icon = "⚡" },
                    new() { Id = "sensor-power", Name = "Power", Icon = "🔋" },
                    new() { Id = "sensor-clock", Name = "Clocks", Icon = "⏱️" },
                    new() { Id = "sensor-load", Name = "Utilization", Icon = "📊" },
                }
            },
            new CategoryNode
            {
                Id = "benchmark",
                Name = "Benchmark",
                Icon = "🏁",
                Children = new ObservableCollection<CategoryNode>
                {
                    new() { Id = "bench-memory", Name = "Memory Benchmark", Icon = "💾" },
                    new() { Id = "bench-cpu", Name = "CPU Benchmark", Icon = "⚡" }
                }
            }
        };
    }

    public void OnCategorySelected(CategoryNode node)
    {
        SelectedCategory = node;
        SelectedCategoryName = node.Name;
        SelectedCategoryIcon = node.Icon;
        CategorySelected?.Invoke(node);
    }

    /// <summary>
    /// IDでカテゴリを検索する（ネストされた子ノードも含む）。
    /// </summary>
    public CategoryNode? FindById(string id)
    {
        return FindInCollection(Categories, id);
    }

    private static CategoryNode? FindInCollection(IEnumerable<CategoryNode> nodes, string id)
    {
        foreach (var node in nodes)
        {
            if (node.Id == id) return node;
            if (node.Children != null)
            {
                var found = FindInCollection(node.Children, id);
                if (found != null) return found;
            }
        }
        return null;
    }
}
