using LibreHardwareMonitor.Hardware;
using SAYA64Extreme.Models;
using System.Collections.ObjectModel;

namespace SAYA64Extreme.Services;

public class SensorService : IDisposable
{
    private readonly Computer _computer;
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
            IsStorageEnabled = true,
            IsNetworkEnabled = true
        };
    }

    public void Open()
    {
        if (!_isOpen)
        {
            _computer.Open();
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

        foreach (var hardware in _computer.Hardware)
        {
            hardware.Update();
            foreach (var subHardware in hardware.SubHardware)
            {
                subHardware.Update();
                AddSensors(readings, subHardware.Sensors, subHardware.Name);
            }
            AddSensors(readings, hardware.Sensors, hardware.Name);
        }
        return readings;
    }

    public void UpdateReadings(ObservableCollection<SensorReading> existingReadings)
    {
        if (!_isOpen) return;

        _sensorMap.Clear();
        foreach (var hardware in _computer.Hardware)
        {
            hardware.Update();
            foreach (var subHardware in hardware.SubHardware)
            {
                subHardware.Update();
                foreach (var sensor in subHardware.Sensors)
                    _sensorMap[$"{subHardware.Name}/{sensor.Name}/{sensor.SensorType}"] = sensor;
            }
            foreach (var sensor in hardware.Sensors)
                _sensorMap[$"{hardware.Name}/{sensor.Name}/{sensor.SensorType}"] = sensor;
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
