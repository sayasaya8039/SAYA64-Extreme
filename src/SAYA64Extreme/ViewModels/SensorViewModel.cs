using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAYA64Extreme.Models;
using SAYA64Extreme.Services;

namespace SAYA64Extreme.ViewModels;

public partial class SensorViewModel : ObservableObject, IDisposable
{
    private readonly SensorService _sensorService;
    private Timer? _backgroundTimer;
    private ObservableCollection<SensorReading>? _allReadings;
    private ObservableCollection<SensorReading>? _currentFilteredView;
    private bool _isMonitoring;
    private bool _disposed;
    private string _lastFilterCategory = "sensor";

    [ObservableProperty]
    private ObservableCollection<SensorReading> currentReadings = new();

    [ObservableProperty]
    private string sensorUpdateInfo = "";

    [ObservableProperty]
    private bool isActive;

    [ObservableProperty]
    private string currentFilter = "all";

    [ObservableProperty]
    private int readingCount;

    public event Action? SensorUpdated;

    public SensorViewModel(SensorService sensorService)
    {
        _sensorService = sensorService;
    }

    public void StartMonitoring()
    {
        if (_isMonitoring) return;

        try
        {
            _sensorService.Open();
            _allReadings = _sensorService.GetSensorReadings();

            _backgroundTimer = new Timer(OnTimerTickBackground, null,
                TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            _isMonitoring = true;
            IsActive = true;
            SensorUpdateInfo = "Sensor monitoring active";
        }
        catch (Exception ex)
        {
            SensorUpdateInfo = $"Sensor init failed: {ex.Message}";
            IsActive = false;
        }
    }

    public void StopMonitoring()
    {
        _backgroundTimer?.Dispose();
        _backgroundTimer = null;
        _isMonitoring = false;
        IsActive = false;
        SensorUpdateInfo = "Sensor monitoring stopped";
    }

    public ObservableCollection<SensorReading> GetFilteredReadings(string sensorCategoryId)
    {
        if (_allReadings == null) return new ObservableCollection<SensorReading>();

        CurrentFilter = sensorCategoryId;
        _lastFilterCategory = sensorCategoryId;

        var filteredType = sensorCategoryId switch
        {
            "sensor-temp" => "Temperature",
            "sensor-fan" => "Fan",
            "sensor-voltage" => "Voltage",
            "sensor-power" => "Power",
            "sensor-clock" => "Clock",
            "sensor-load" => "Load",
            "sensor" => null,
            _ => null
        };

        IEnumerable<SensorReading> filtered = filteredType != null
            ? _allReadings.Where(r => r.SensorType == filteredType)
            : _allReadings;

        _currentFilteredView = new ObservableCollection<SensorReading>(filtered);
        CurrentReadings = _currentFilteredView;
        ReadingCount = CurrentReadings.Count;
        return CurrentReadings;
    }

    [RelayCommand]
    private void RefreshSensors()
    {
        try
        {
            _sensorService.Close();
            _sensorService.Open();
            _allReadings = _sensorService.GetSensorReadings();

            if (!string.IsNullOrEmpty(_lastFilterCategory))
                GetFilteredReadings(_lastFilterCategory);

            SensorUpdateInfo = $"Refreshed: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            SensorUpdateInfo = $"Refresh error: {ex.Message}";
        }
    }

    private void OnTimerTickBackground(object? state)
    {
        try
        {
            if (_allReadings != null)
            {
                _sensorService.UpdateReadings(_allReadings);

                System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
                {
                    SensorUpdateInfo = $"Last update: {DateTime.Now:HH:mm:ss}";
                    SensorUpdated?.Invoke();
                });
            }
        }
        catch { }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        StopMonitoring();
        _sensorService.Dispose();
        GC.SuppressFinalize(this);
    }
}
