using LibreHardwareMonitor.Hardware;
using SAYA64Extreme.Models;
using System.Collections.ObjectModel;

namespace SAYA64Extreme.Services;

/// <summary>
/// LibreHardwareMonitor の IVisitor 実装。
/// hardware.Update() + SubHardware の再帰的更新を確実に行う。
/// </summary>
internal sealed class UpdateVisitor : IVisitor
{
    public void VisitComputer(IComputer computer)
    {
        computer.Traverse(this);
    }

    public void VisitHardware(IHardware hardware)
    {
        hardware.Update();
        foreach (var sub in hardware.SubHardware)
            sub.Accept(this);
    }

    public void VisitSensor(ISensor sensor) { }
    public void VisitParameter(IParameter parameter) { }
}

public class SensorService : IDisposable
{
    private readonly Computer _computer;
    private readonly UpdateVisitor _visitor = new();
    private readonly Dictionary<string, ISensor> _sensorMap = new(256);
    private bool _isOpen;

    public SensorService()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = true,
            IsStorageEnabled = true,
            IsNetworkEnabled = true
        };
    }

    public void Open()
    {
        if (!_isOpen)
        {
            _computer.Open();
            _computer.Accept(_visitor);
            _isOpen = true;
        }
    }

    public void Close()
    {
        if (_isOpen)
        {
            _computer.Close();
            _isOpen = false;
        }
    }

    public ObservableCollection<SensorReading> GetSensorReadings()
    {
        var readings = new ObservableCollection<SensorReading>();
        if (!_isOpen) return readings;

        // Visitor で全ハードウェアを再帰的に更新
        _computer.Accept(_visitor);

        foreach (var hardware in _computer.Hardware)
        {
            CollectSensors(readings, hardware);
        }
        return readings;
    }

    public void UpdateReadings(ObservableCollection<SensorReading> existingReadings)
    {
        if (!_isOpen) return;

        // Visitor で全ハードウェアを再帰的に更新
        _computer.Accept(_visitor);

        _sensorMap.Clear();
        foreach (var hardware in _computer.Hardware)
        {
            BuildSensorMap(hardware);
        }

        foreach (var reading in existingReadings)
        {
            var key = $"{reading.HardwareName}/{reading.Name}/{reading.SensorType}";
            if (_sensorMap.TryGetValue(key, out var sensor) && sensor.Value.HasValue)
            {
                var formatted = FormatSensorValue(sensor.Value.Value, sensor.SensorType);
                if (reading.CurrentValue != formatted)
                    reading.CurrentValue = formatted;

                if (sensor.Min.HasValue)
                {
                    var minFormatted = FormatSensorValue(sensor.Min.Value, sensor.SensorType);
                    if (reading.MinValue != minFormatted)
                        reading.MinValue = minFormatted;
                }
                if (sensor.Max.HasValue)
                {
                    var maxFormatted = FormatSensorValue(sensor.Max.Value, sensor.SensorType);
                    if (reading.MaxValue != maxFormatted)
                        reading.MaxValue = maxFormatted;
                }
            }
        }
    }

    /// <summary>
    /// ハードウェアとその全SubHardwareからセンサーを再帰的に収集
    /// </summary>
    private void CollectSensors(ObservableCollection<SensorReading> readings, IHardware hardware)
    {
        AddSensors(readings, hardware.Sensors, hardware.Name);

        foreach (var sub in hardware.SubHardware)
        {
            CollectSensors(readings, sub);
        }
    }

    /// <summary>
    /// ハードウェアとその全SubHardwareのセンサーをマップに再帰的に登録
    /// </summary>
    private void BuildSensorMap(IHardware hardware)
    {
        foreach (var sensor in hardware.Sensors)
            _sensorMap[$"{hardware.Name}/{sensor.Name}/{sensor.SensorType}"] = sensor;

        foreach (var sub in hardware.SubHardware)
        {
            BuildSensorMap(sub);
        }
    }

    private void AddSensors(ObservableCollection<SensorReading> readings, ISensor[] sensors, string hardwareName)
    {
        foreach (var sensor in sensors)
        {
            var reading = new SensorReading
            {
                Name = sensor.Name,
                SensorType = sensor.SensorType.ToString(),
                HardwareName = hardwareName,
                Unit = GetUnit(sensor.SensorType),
                CurrentValue = sensor.Value.HasValue ? FormatSensorValue(sensor.Value.Value, sensor.SensorType) : "N/A",
                MinValue = sensor.Min.HasValue ? FormatSensorValue(sensor.Min.Value, sensor.SensorType) : "N/A",
                MaxValue = sensor.Max.HasValue ? FormatSensorValue(sensor.Max.Value, sensor.SensorType) : "N/A"
            };
            readings.Add(reading);
        }
    }

    private static string FormatSensorValue(float value, SensorType type) => type switch
    {
        SensorType.Temperature => $"{value:F1} °C",
        SensorType.Fan => $"{value:F0} RPM",
        SensorType.Voltage => $"{value:F3} V",
        SensorType.Power => $"{value:F1} W",
        SensorType.Clock => $"{value:F0} MHz",
        SensorType.Load => $"{value:F1} %",
        SensorType.Data => $"{value:F2} GB",
        SensorType.SmallData => $"{value:F0} MB",
        SensorType.Throughput => $"{value / 1024:F1} KB/s",
        _ => $"{value:F2}"
    };

    private static string GetUnit(SensorType type) => type switch
    {
        SensorType.Temperature => "°C",
        SensorType.Fan => "RPM",
        SensorType.Voltage => "V",
        SensorType.Power => "W",
        SensorType.Clock => "MHz",
        SensorType.Load => "%",
        SensorType.Data => "GB",
        SensorType.SmallData => "MB",
        SensorType.Throughput => "KB/s",
        _ => ""
    };

    public void Dispose()
    {
        Close();
        GC.SuppressFinalize(this);
    }
}
