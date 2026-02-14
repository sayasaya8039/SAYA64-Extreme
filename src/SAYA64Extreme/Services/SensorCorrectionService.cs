using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using SAYA64Extreme.Models;

namespace SAYA64Extreme.Services;

public class SensorCorrectionService
{
    private readonly Dictionary<string, SensorCorrection> _corrections = new();
    private readonly string _configPath;

    public SensorCorrectionService()
    {
        _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sensor_corrections.json");
        LoadCorrections();
    }

    public void ApplyCorrections(ObservableCollection<SensorReading> readings)
    {
        foreach (var reading in readings)
        {
            var sensorId = $"{reading.HardwareName}/{reading.Name}/{reading.SensorType}";
            if (!_corrections.TryGetValue(sensorId, out var correction))
                continue;

            if (!TryParseNumericValue(reading.CurrentValue, out var numericValue, out var suffix))
                continue;

            var corrected = (numericValue + correction.Offset) * correction.Ratio;
            reading.CurrentValue = FormatCorrectedValue(corrected, suffix);
        }
    }

    public void SetCorrection(string sensorId, double offset, double ratio)
    {
        _corrections[sensorId] = new SensorCorrection
        {
            SensorId = sensorId,
            Offset = offset,
            Ratio = ratio
        };
    }

    public SensorCorrection? GetCorrection(string sensorId)
    {
        return _corrections.TryGetValue(sensorId, out var correction) ? correction : null;
    }

    public void RemoveCorrection(string sensorId)
    {
        _corrections.Remove(sensorId);
    }

    public void LoadCorrections()
    {
        try
        {
            if (!File.Exists(_configPath)) return;

            var json = File.ReadAllText(_configPath);
            var list = JsonSerializer.Deserialize<List<SensorCorrection>>(json);
            if (list == null) return;

            _corrections.Clear();
            foreach (var c in list)
                _corrections[c.SensorId] = c;
        }
        catch
        {
            // 設定ファイル読み込み失敗時は空の状態で続行
        }
    }

    public void SaveCorrections()
    {
        try
        {
            var list = _corrections.Values.ToList();
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(list, options);
            File.WriteAllText(_configPath, json);
        }
        catch
        {
            // 保存失敗時は無視
        }
    }

    private static bool TryParseNumericValue(string formatted, out double value, out string suffix)
    {
        value = 0;
        suffix = "";

        if (string.IsNullOrWhiteSpace(formatted) || formatted == "N/A")
            return false;

        // "123.4 °C" のような形式からパース
        var trimmed = formatted.Trim();
        var spaceIndex = trimmed.IndexOf(' ');
        if (spaceIndex > 0)
        {
            suffix = trimmed[spaceIndex..];
            return double.TryParse(trimmed[..spaceIndex], out value);
        }

        return double.TryParse(trimmed, out value);
    }

    private static string FormatCorrectedValue(double value, string suffix)
    {
        // 元のフォーマットに合わせた精度で出力
        var trimmedSuffix = suffix.Trim();
        var format = trimmedSuffix switch
        {
            "°C" => $"{value:F1}",
            "RPM" => $"{value:F0}",
            "V" => $"{value:F3}",
            "W" => $"{value:F1}",
            "MHz" => $"{value:F0}",
            "%" => $"{value:F1}",
            "GB" => $"{value:F2}",
            "MB" => $"{value:F0}",
            "KB/s" => $"{value:F1}",
            _ => $"{value:F2}"
        };

        return string.IsNullOrEmpty(suffix) ? format : $"{format}{suffix}";
    }
}
