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
    private readonly MemoryBenchmarkService _memBenchService = new();
    private readonly CpuBenchmarkService _cpuBenchService = new();
    private readonly FpuBenchmarkService _fpuBenchService = new();
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
        StatusText = LanguageService.GetString("Status_Ready");
    }

    public void Cleanup()
    {
        Dispose();
    }

    public void OnCategorySelected(CategoryNode node)
    {
        _categoryViewModel.OnCategorySelected(node);
    }

    public void ChangeTheme(string theme)
    {
        ThemeService.SetTheme(theme);
        StatusText = LanguageService.GetString("Status_ThemeChanged");
    }

    public void ChangeLanguage(string lang)
    {
        LanguageService.SetLanguage(lang);

        // カテゴリツリーを再構築して言語を反映
        _staticDataCache.Clear();
        _categoryViewModel.BuildCategoryTree();
        Categories = _categoryViewModel.Categories;

        // 現在のビューを再読み込み
        if (_currentCategoryId.StartsWith("sensor"))
            LoadSensorData(_currentCategoryId);
        else if (_currentCategoryId.StartsWith("bench"))
            LoadBenchmarkView(_currentCategoryId);
        else
            LoadStaticData(_currentCategoryId);

        StatusText = LanguageService.GetString("Status_LanguageChanged");
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
        StatusText = LanguageService.GetString("Status_Loading");

        if (categoryId != "processes" && _staticDataCache.TryGetValue(categoryId, out var cached))
        {
            CurrentItems = new ObservableCollection<object>(cached);
            StatusText = $"{categoryId} - {cached.Count} {LanguageService.GetString("Status_ItemsLoaded")} ({LanguageService.GetString("Status_Cached")})";
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
            _ => new List<SystemItem> { new() { Name = "Info", Value = LanguageService.GetString("Status_SelectCategory") } }
        };

        if (categoryId != "processes")
            _staticDataCache[categoryId] = items;
        CurrentItems = new ObservableCollection<object>(items);
        StatusText = $"{categoryId} - {items.Count} {LanguageService.GetString("Status_ItemsLoaded")}";
    }

    private void LoadSensorData(string sensorCategoryId)
    {
        var readings = _sensorViewModel.GetFilteredReadings(sensorCategoryId);
        CurrentItems = new ObservableCollection<object>(readings);
        StatusText = $"Sensor - {readings.Count} {LanguageService.GetString("Status_SensorReadings")}";
    }

    private void LoadBenchmarkView(string benchId)
    {
        if (_staticDataCache.TryGetValue(benchId, out var cached))
        {
            CurrentItems = new ObservableCollection<object>(cached);
            StatusText = $"Benchmark - {cached.Count} {LanguageService.GetString("Status_BenchmarkResults")} ({LanguageService.GetString("Status_Cached")})";
            return;
        }

        var items = new List<SystemItem>
        {
            new() { Name = "Status", Value = LanguageService.GetString("Status_BenchmarkReady"), CategoryId = benchId }
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
        StatusText = LanguageService.GetString("Status_Refreshing");
        _staticDataCache.Clear();

        _sensorViewModel.RefreshSensorsCommand.Execute(null);

        if (_currentCategoryId.StartsWith("sensor"))
            LoadSensorData(_currentCategoryId);
        else if (!_currentCategoryId.StartsWith("bench"))
            LoadStaticData(_currentCategoryId);

        StatusText = LanguageService.GetString("Status_RefreshComplete");
    }

    [RelayCommand]
    private void ExportReport()
    {
        try
        {
            StatusText = LanguageService.GetString("Status_GeneratingReport");
            var filePath = _reportService.ExportToFile();
            StatusText = $"{LanguageService.GetString("Status_ReportSaved")}{filePath}";
        }
        catch (Exception ex)
        {
            StatusText = $"{LanguageService.GetString("Status_ReportError")}{ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RunBenchmark()
    {
        var benchId = _currentCategoryId.StartsWith("bench") ? _currentCategoryId : "bench-memory";

        try
        {
            List<SystemItem> results;

            switch (benchId)
            {
                case "bench-memory":
                    StatusText = LanguageService.GetString("Status_RunningMemBench");
                    results = await Task.Run(() => ConvertBenchResults(_memBenchService.RunAll()));
                    break;
                case "bench-cpu":
                    StatusText = LanguageService.GetString("Status_RunningCpuBench");
                    results = await Task.Run(() => ConvertBenchResults(_cpuBenchService.RunAll()));
                    break;
                case "bench-fpu":
                    StatusText = LanguageService.GetString("Status_RunningFpuBench");
                    results = await Task.Run(() => ConvertBenchResults(_fpuBenchService.RunAll()));
                    break;
                default:
                    StatusText = LanguageService.GetString("Status_RunningBenchmark");
                    results = await Task.Run(() => _benchmarkService.RunMemoryBenchmark());
                    break;
            }

            _staticDataCache[benchId] = results;
            CurrentItems = new ObservableCollection<object>(results);
            StatusText = $"{LanguageService.GetString("Status_BenchmarkComplete")} - {results.Count} {LanguageService.GetString("Status_BenchmarkResults")}";
        }
        catch (Exception ex)
        {
            StatusText = $"{LanguageService.GetString("Status_BenchmarkError")}{ex.Message}";
        }
    }

    private static List<SystemItem> ConvertBenchResults(List<BenchmarkResult> benchResults)
    {
        var items = new List<SystemItem>();
        foreach (var r in benchResults)
        {
            items.Add(new SystemItem { Name = r.TestName, Value = $"{r.Score} {r.Unit}", CategoryId = r.Category });
            items.Add(new SystemItem { Name = "  Duration", Value = $"{r.DurationMs:F0} ms", CategoryId = r.Category });
            items.Add(new SystemItem { Name = "  Threads", Value = r.ThreadCount.ToString(), CategoryId = r.Category });
            items.Add(new SystemItem { Name = "  Info", Value = r.SimdInfo, CategoryId = r.Category });
            items.Add(new SystemItem { Name = "---", Value = "---", CategoryId = r.Category });
        }
        if (items.Count > 0) items.RemoveAt(items.Count - 1);
        return items;
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
