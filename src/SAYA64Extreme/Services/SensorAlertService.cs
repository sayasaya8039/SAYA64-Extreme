using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using SAYA64Extreme.Models;

namespace SAYA64Extreme.Services;

public class SensorAlertService
{
    private readonly Dictionary<string, SensorAlert> _alerts = new();
    private readonly string _configPath;

    public event Action<SensorAlert, SensorReading>? AlertTriggered;

    public SensorAlertService()
    {
        _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sensor_alerts.json");
        LoadAlerts();
    }

    public void CheckAlerts(ObservableCollection<SensorReading> readings)
    {
        foreach (var reading in readings)
        {
            var sensorId = $"{reading.HardwareName}/{reading.Name}/{reading.SensorType}";
            if (!_alerts.TryGetValue(sensorId, out var alert))
                continue;

            if (!alert.IsEnabled)
                continue;

            if (!TryParseNumericValue(reading.CurrentValue, out var numericValue))
                continue;

            var triggered = false;

            if (alert.MinThreshold.HasValue && numericValue < alert.MinThreshold.Value)
                triggered = true;

            if (alert.MaxThreshold.HasValue && numericValue > alert.MaxThreshold.Value)
                triggered = true;

            if (triggered)
                AlertTriggered?.Invoke(alert, reading);
        }
    }

    public void SetAlert(string sensorId, string sensorName, double? minThreshold, double? maxThreshold,
        AlertAction action = AlertAction.Warning)
    {
        _alerts[sensorId] = new SensorAlert
        {
            SensorId = sensorId,
            SensorName = sensorName,
            MinThreshold = minThreshold,
            MaxThreshold = maxThreshold,
            Action = action,
            IsEnabled = true
        };
    }

    public SensorAlert? GetAlert(string sensorId)
    {
        return _alerts.TryGetValue(sensorId, out var alert) ? alert : null;
    }

    public void RemoveAlert(string sensorId)
    {
        _alerts.Remove(sensorId);
    }

    public void EnableAlert(string sensorId, bool enabled)
    {
        if (_alerts.TryGetValue(sensorId, out var alert))
            alert.IsEnabled = enabled;
    }

    public IReadOnlyCollection<SensorAlert> GetAllAlerts()
    {
        return _alerts.Values.ToList().AsReadOnly();
    }

    public void LoadAlerts()
    {
        try
        {
            if (!File.Exists(_configPath)) return;

            var json = File.ReadAllText(_configPath);
            var list = JsonSerializer.Deserialize<List<SensorAlert>>(json);
            if (list == null) return;

            _alerts.Clear();
            foreach (var a in list)
                _alerts[a.SensorId] = a;
        }
        catch
        {
            // 設定ファイル読み込み失敗時は空の状態で続行
        }
    }

    public void SaveAlerts()
    {
        try
        {
            var list = _alerts.Values.ToList();
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(list, options);
            File.WriteAllText(_configPath, json);
        }
        catch
        {
            // 保存失敗時は無視
        }
    }

    private static bool TryParseNumericValue(string formatted, out double value)
    {
        value = 0;

        if (string.IsNullOrWhiteSpace(formatted) || formatted == "N/A")
            return false;

        var trimmed = formatted.Trim();
        var spaceIndex = trimmed.IndexOf(' ');
        if (spaceIndex > 0)
            return double.TryParse(trimmed[..spaceIndex], out value);

        return double.TryParse(trimmed, out value);
    }
}
