using SAYA64Extreme.Models;
using System.Collections.ObjectModel;

namespace SAYA64Extreme.Services;

/// <summary>
/// Static（WMI）とDynamic（センサー）データ取得を統括するマネージャー
/// </summary>
public class ProbeManager : IDisposable
{
    private readonly WmiService _wmiService;
    private readonly SensorService _sensorService;
    private ObservableCollection<SensorReading>? _cachedReadings;

    public ProbeManager()
    {
        _wmiService = new WmiService();
        _sensorService = new SensorService();
    }

    /// <summary>
    /// センサー監視を開始する
    /// </summary>
    public void Initialize()
    {
        _sensorService.Open();
    }

    // --- Static Data (WMI) ---

    public List<SystemItem> GetStaticData(string categoryId)
    {
        return categoryId switch
        {
            "computer" => _wmiService.GetComputerSummary(),
            "cpu" => _wmiService.GetCpuInfo(),
            "motherboard" => _wmiService.GetMotherboardInfo(),
            "memory" => _wmiService.GetMemoryInfo(),
            "os" => _wmiService.GetOsInfo(),
            _ => new List<SystemItem>()
        };
    }

    public List<SystemItem> GetAllStaticData()
    {
        var all = new List<SystemItem>();
        all.AddRange(_wmiService.GetComputerSummary());
        all.AddRange(_wmiService.GetCpuInfo());
        all.AddRange(_wmiService.GetMotherboardInfo());
        all.AddRange(_wmiService.GetMemoryInfo());
        all.AddRange(_wmiService.GetOsInfo());
        return all;
    }

    // --- Dynamic Data (Sensors) ---

    public ObservableCollection<SensorReading> GetSensorReadings()
    {
        _cachedReadings = _sensorService.GetSensorReadings();
        return _cachedReadings;
    }

    /// <summary>
    /// 既存のセンサー読み取り値をインプレース更新する（UIバインディング維持）
    /// </summary>
    public void RefreshSensorReadings()
    {
        if (_cachedReadings != null)
        {
            _sensorService.UpdateReadings(_cachedReadings);
        }
    }

    /// <summary>
    /// 特定のセンサータイプのみフィルタして取得
    /// </summary>
    public ObservableCollection<SensorReading> GetSensorReadingsByType(string sensorType)
    {
        var all = GetSensorReadings();
        var filtered = new ObservableCollection<SensorReading>();
        foreach (var reading in all)
        {
            if (reading.SensorType == sensorType)
                filtered.Add(reading);
        }
        return filtered;
    }

    /// <summary>
    /// 特定ハードウェアのセンサーのみフィルタして取得
    /// </summary>
    public ObservableCollection<SensorReading> GetSensorReadingsByHardware(string hardwareName)
    {
        var all = GetSensorReadings();
        var filtered = new ObservableCollection<SensorReading>();
        foreach (var reading in all)
        {
            if (reading.HardwareName == hardwareName)
                filtered.Add(reading);
        }
        return filtered;
    }

    public void Dispose()
    {
        _sensorService.Dispose();
        GC.SuppressFinalize(this);
    }
}
