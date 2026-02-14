using System.Collections.ObjectModel;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using Microsoft.Win32;
using SAYA64Extreme.Models;

namespace SAYA64Extreme.Services;

public class SensorSharingService : IDisposable
{
    private const string MemoryMappedFileName = "SAYA64_SensorValues";
    private const int MemoryMappedFileSize = 65536; // 64KB
    private const string RegistryKeyPath = @"Software\SAYA64\SensorValues";

    private MemoryMappedFile? _mmf;
    private bool _isSharing;
    private bool _shareToMemory;
    private bool _shareToRegistry;
    private bool _disposed;

    public void StartSharing(bool shareToMemory = true, bool shareToRegistry = false)
    {
        if (_isSharing) return;

        _shareToMemory = shareToMemory;
        _shareToRegistry = shareToRegistry;

        if (_shareToMemory)
        {
            try
            {
                _mmf = MemoryMappedFile.CreateOrOpen(MemoryMappedFileName, MemoryMappedFileSize);
            }
            catch
            {
                _mmf = null;
            }
        }

        _isSharing = true;
    }

    public void StopSharing()
    {
        if (!_isSharing) return;

        _isSharing = false;

        _mmf?.Dispose();
        _mmf = null;

        // レジストリキーをクリーンアップ
        if (_shareToRegistry)
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(RegistryKeyPath, throwOnMissingSubKey: false);
            }
            catch
            {
                // クリーンアップ失敗は無視
            }
        }
    }

    public void Update(ObservableCollection<SensorReading> readings)
    {
        if (!_isSharing) return;

        if (_shareToMemory)
            WriteToMemoryMappedFile(readings);

        if (_shareToRegistry)
            WriteToRegistry(readings);
    }

    private void WriteToMemoryMappedFile(ObservableCollection<SensorReading> readings)
    {
        if (_mmf == null) return;

        try
        {
            var xml = BuildXml(readings);
            var bytes = Encoding.UTF8.GetBytes(xml);

            // サイズ制限チェック（64KB - 4バイトのヘッダ）
            var writeLength = Math.Min(bytes.Length, MemoryMappedFileSize - 4);

            using var accessor = _mmf.CreateViewAccessor(0, MemoryMappedFileSize);
            // 先頭4バイトにデータ長を書き込み
            accessor.Write(0, writeLength);
            // データ本体を書き込み
            accessor.WriteArray(4, bytes, 0, writeLength);
        }
        catch
        {
            // 共有メモリ書き込み失敗は無視
        }
    }

    private void WriteToRegistry(ObservableCollection<SensorReading> readings)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            if (key == null) return;

            foreach (var reading in readings)
            {
                var valueName = $"{reading.HardwareName}_{reading.Name}_{reading.SensorType}";
                // レジストリ値名の不正文字を置換
                valueName = valueName.Replace('\\', '_').Replace('/', '_');
                key.SetValue(valueName, reading.CurrentValue);
            }
        }
        catch
        {
            // レジストリ書き込み失敗は無視
        }
    }

    private static string BuildXml(ObservableCollection<SensorReading> readings)
    {
        var sb = new StringBuilder();
        sb.Append("<saya64>");

        foreach (var reading in readings)
        {
            sb.Append("<sensor>");
            sb.Append("<id>");
            sb.Append(EscapeXml($"{reading.HardwareName}/{reading.Name}"));
            sb.Append("</id>");
            sb.Append("<value>");
            sb.Append(EscapeXml(reading.CurrentValue));
            sb.Append("</value>");
            sb.Append("</sensor>");
        }

        sb.Append("</saya64>");
        return sb.ToString();
    }

    private static string EscapeXml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        StopSharing();
        GC.SuppressFinalize(this);
    }
}
