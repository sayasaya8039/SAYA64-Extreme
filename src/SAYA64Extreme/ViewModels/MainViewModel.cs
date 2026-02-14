using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAYA64Extreme.Models;
using SAYA64Extreme.Services;

namespace SAYA64Extreme.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly WmiService _wmiService = new();
    private readonly SoftwareService _softwareService = new();
    private readonly BenchmarkService _benchmarkService = new();
    private readonly ReportService _reportService;
    private readonly SensorService _sensorService = new();
    private readonly CategoryViewModel _categoryViewModel;
    private readonly SensorViewModel _sensorViewModel;
    private readonly Dictionary<string, List<SystemItem>> _staticDataCache = new();
    private string _currentCategoryId = "computer";
    private bool _disposed;

    [ObservableProperty]
    private ObservableCollection<CategoryNode> categories = new();

    [ObservableProperty]
    private ObservableCollection<object> currentItems = new();

    [ObservableProperty]
    private string selectedCategoryName = "Computer";

    [ObservableProperty]
    private string selectedCategoryIcon = "\U0001F4BB";

    [ObservableProperty]
    private string statusText = "Ready";

    [ObservableProperty]
    private string sensorUpdateInfo = "";

    public MainViewModel()
    {
        _categoryViewModel = new CategoryViewModel();
        _sensorViewModel = new SensorViewModel(_sensorService);
        _reportService = new ReportService(_wmiService);
        _categoryViewModel.CategorySelected += OnCategoryChangedInternal;
        _sensorViewModel.SensorUpdated += OnSensorDataUpdated;
    }

    public CategoryViewModel CategoryVM => _categoryViewModel;
    public SensorViewModel SensorVM => _sensorViewModel;

    public void Initialize()
    {
        _categoryViewModel.BuildCategoryTree();
        Categories = _categoryViewModel.Categories;
        LoadStaticData("computer");
        _sensorViewModel.StartMonitoring();
        StatusText = "Ready";
    }

    public void Cleanup()
    {
        Dispose();
    }

    public void OnCategorySelected(CategoryNode node)
    {
        _categoryViewModel.OnCategorySelected(node);
    }

    private void OnCategoryChangedInternal(CategoryNode node)
    {
        _currentCategoryId = node.Id;
        SelectedCategoryName = node.Name;
        SelectedCategoryIcon = node.Icon;

        if (node.Id.StartsWith("sensor"))
        {
            LoadSensorData(node.Id);
        }
        else if (node.Id.StartsWith("bench"))
        {
            LoadBenchmarkView(node.Id);
        }
        else
        {
            LoadStaticData(node.Id);
        }
    }

    private void LoadStaticData(string categoryId)
    {
        StatusText = $"Loading {categoryId} data...";

        // processes はキャッシュしない（リアルタイム性が必要）
        if (categoryId != "processes" && _staticDataCache.TryGetValue(categoryId, out var cached))
        {
            CurrentItems = new ObservableCollection<object>(cached);
            StatusText = $"{categoryId} - {cached.Count} items (cached)";
            return;
        }

        var items = categoryId switch
        {
            "computer" or "summary" => _wmiService.GetComputerSummary(),
            "cpu" => _wmiService.GetCpuInfo(),
            "motherboard" => _wmiService.GetMotherboardInfo(),
            "memory" => _wmiService.GetMemoryInfo(),
            "os" => _wmiService.GetOsInfo(),
            "storage" or "disk" => _wmiService.GetStorageInfo(),
            "smart" => _wmiService.GetSmartInfo(),
            "display" or "gpu" => _wmiService.GetGpuInfo(),
            "monitor" => _wmiService.GetMonitorInfo(),
            "network" => _wmiService.GetNetworkInfo(),
            "software" or "programs" => _softwareService.GetInstalledPrograms(),
            "processes" => _softwareService.GetProcessList(),
            "devices" or "usb" => _softwareService.GetUsbDevices(),
            _ => new List<SystemItem> { new() { Name = "Info", Value = "Select a category from the tree" } }
        };

        if (categoryId != "processes")
            _staticDataCache[categoryId] = items;
        CurrentItems = new ObservableCollection<object>(items);
        StatusText = $"{categoryId} - {items.Count} items loaded";
    }

    private void LoadSensorData(string sensorCategoryId)
    {
        var readings = _sensorViewModel.GetFilteredReadings(sensorCategoryId);
        CurrentItems = new ObservableCollection<object>(readings);
        StatusText = $"Sensor - {readings.Count} readings";
    }

    private void LoadBenchmarkView(string benchId)
    {
        var items = new List<SystemItem>
        {
            new() { Name = "Status", Value = "Click 'Run Benchmark' from Tools menu to start", CategoryId = benchId }
        };
        CurrentItems = new ObservableCollection<object>(items);
        StatusText = $"Benchmark - {benchId}";
    }

    private void OnSensorDataUpdated()
    {
        SensorUpdateInfo = _sensorViewModel.SensorUpdateInfo;
    }

    [RelayCommand]
    private void Refresh()
    {
        StatusText = "Refreshing...";
        _staticDataCache.Clear();

        _sensorViewModel.RefreshSensorsCommand.Execute(null);

        if (_currentCategoryId.StartsWith("sensor"))
            LoadSensorData(_currentCategoryId);
        else if (!_currentCategoryId.StartsWith("bench"))
            LoadStaticData(_currentCategoryId);

        StatusText = "Refresh complete";
    }

    [RelayCommand]
    private void ExportReport()
    {
        try
        {
            StatusText = "Generating report...";
            var filePath = _reportService.ExportToFile();
            StatusText = $"Report saved: {filePath}";
        }
        catch (Exception ex)
        {
            StatusText = $"Report error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void RunBenchmark()
    {
        StatusText = "Running memory benchmark...";
        try
        {
            var results = _benchmarkService.RunMemoryBenchmark();
            _staticDataCache["bench-memory"] = results;
            CurrentItems = new ObservableCollection<object>(results);
            StatusText = $"Benchmark complete - {results.Count} results";
        }
        catch (Exception ex)
        {
            StatusText = $"Benchmark error: {ex.Message}";
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _categoryViewModel.CategorySelected -= OnCategoryChangedInternal;
        _sensorViewModel.SensorUpdated -= OnSensorDataUpdated;
        _sensorViewModel.Dispose();
        GC.SuppressFinalize(this);
    }
}
