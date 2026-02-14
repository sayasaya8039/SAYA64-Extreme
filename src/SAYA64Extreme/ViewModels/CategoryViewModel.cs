using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SAYA64Extreme.Models;
using SAYA64Extreme.Services;

namespace SAYA64Extreme.ViewModels;

public partial class CategoryViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<CategoryNode> categories = new();

    [ObservableProperty]
    private CategoryNode? selectedCategory;

    [ObservableProperty]
    private string selectedCategoryName = "Computer";

    [ObservableProperty]
    private string selectedCategoryIcon = "\U0001F4BB";

    public event Action<CategoryNode>? CategorySelected;

    private static string L(string key) => LanguageService.GetString(key);

    public void BuildCategoryTree()
    {
        Categories = new ObservableCollection<CategoryNode>
        {
            new CategoryNode
            {
                Id = "computer",
                Name = L("Cat_Computer"),
                Icon = "\U0001F4BB",
                Children = new ObservableCollection<CategoryNode>
                {
                    new() { Id = "summary", Name = L("Cat_Summary"), Icon = "\U0001F4CB" }
                }
            },
            new CategoryNode
            {
                Id = "motherboard",
                Name = L("Cat_Motherboard"),
                Icon = "\U0001F527",
                Children = new ObservableCollection<CategoryNode>
                {
                    new() { Id = "cpu", Name = L("Cat_CPU"), Icon = "\u26A1" },
                    new() { Id = "memory", Name = L("Cat_Memory"), Icon = "\U0001F4BE" },
                    new() { Id = "spd", Name = L("Cat_SPD"), Icon = "\U0001F4CA" }
                }
            },
            new CategoryNode
            {
                Id = "display",
                Name = L("Cat_Display"),
                Icon = "\U0001F5A5\uFE0F",
                Children = new ObservableCollection<CategoryNode>
                {
                    new() { Id = "gpu", Name = L("Cat_GPU"), Icon = "\U0001F3AE" },
                    new() { Id = "monitor", Name = L("Cat_Monitor"), Icon = "\U0001F4FA" }
                }
            },
            new CategoryNode
            {
                Id = "storage",
                Name = L("Cat_Storage"),
                Icon = "\U0001F4BF",
                Children = new ObservableCollection<CategoryNode>
                {
                    new() { Id = "disk", Name = L("Cat_Disks"), Icon = "\U0001F5C4\uFE0F" },
                    new() { Id = "smart", Name = L("Cat_SMART"), Icon = "\U0001F50D" }
                }
            },
            new CategoryNode
            {
                Id = "network",
                Name = L("Cat_Network"),
                Icon = "\U0001F310",
            },
            new CategoryNode
            {
                Id = "software",
                Name = L("Cat_Software"),
                Icon = "\U0001F4E6",
                Children = new ObservableCollection<CategoryNode>
                {
                    new() { Id = "programs", Name = L("Cat_InstalledPrograms"), Icon = "\U0001F4CB" },
                    new() { Id = "processes", Name = L("Cat_Processes"), Icon = "\u2699\uFE0F" }
                }
            },
            new CategoryNode
            {
                Id = "devices",
                Name = L("Cat_Devices"),
                Icon = "\U0001F50C",
                Children = new ObservableCollection<CategoryNode>
                {
                    new() { Id = "usb", Name = L("Cat_USBDevices"), Icon = "\U0001F517" }
                }
            },
            new CategoryNode
            {
                Id = "os",
                Name = L("Cat_OS"),
                Icon = "\U0001FA9F",
            },
            new CategoryNode
            {
                Id = "sensor",
                Name = L("Cat_Sensor"),
                Icon = "\U0001F321\uFE0F",
                Children = new ObservableCollection<CategoryNode>
                {
                    new() { Id = "sensor-temp", Name = L("Cat_Temperatures"), Icon = "\U0001F534" },
                    new() { Id = "sensor-fan", Name = L("Cat_CoolingFans"), Icon = "\U0001F300" },
                    new() { Id = "sensor-voltage", Name = L("Cat_Voltages"), Icon = "\u26A1" },
                    new() { Id = "sensor-power", Name = L("Cat_Power"), Icon = "\U0001F50B" },
                    new() { Id = "sensor-clock", Name = L("Cat_Clocks"), Icon = "\u23F1\uFE0F" },
                    new() { Id = "sensor-load", Name = L("Cat_Utilization"), Icon = "\U0001F4CA" },
                }
            },
            new CategoryNode
            {
                Id = "benchmark",
                Name = L("Cat_Benchmark"),
                Icon = "\U0001F3C1",
                Children = new ObservableCollection<CategoryNode>
                {
                    new() { Id = "bench-memory", Name = L("Cat_MemoryBenchmark"), Icon = "\U0001F4BE" },
                    new() { Id = "bench-cpu", Name = L("Cat_CPUBenchmark"), Icon = "\u26A1" },
                    new() { Id = "bench-fpu", Name = L("Cat_FPUBenchmark"), Icon = "\U0001F4D0" }
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
