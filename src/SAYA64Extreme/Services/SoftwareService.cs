using System.Diagnostics;
using System.Management;
using Microsoft.Win32;
using SAYA64Extreme.Models;

namespace SAYA64Extreme.Services;

public class SoftwareService
{
    public List<SystemItem> GetInstalledPrograms()
    {
        var items = new List<SystemItem>(128);
        var programs = new SortedDictionary<string, string>();

        try
        {
            var regPaths = new[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (var regPath in regPaths)
            {
                using var key = Registry.LocalMachine.OpenSubKey(regPath);
                if (key == null) continue;

                foreach (var subKeyName in key.GetSubKeyNames())
                {
                    using var subKey = key.OpenSubKey(subKeyName);
                    if (subKey == null) continue;

                    var name = subKey.GetValue("DisplayName")?.ToString();
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    var version = subKey.GetValue("DisplayVersion")?.ToString() ?? "";
                    var publisher = subKey.GetValue("Publisher")?.ToString() ?? "";
                    var installDate = subKey.GetValue("InstallDate")?.ToString() ?? "";

                    if (!programs.ContainsKey(name))
                        programs[name] = $"{version} | {publisher} | {installDate}";
                }
            }

            foreach (var (name, info) in programs)
            {
                items.Add(new SystemItem { Name = name, Value = info, CategoryId = "programs" });
            }
        }
        catch (Exception ex)
        {
            items.Add(new SystemItem { Name = "Error", Value = ex.Message, CategoryId = "programs" });
        }
        return items;
    }

    public List<SystemItem> GetProcessList()
    {
        var items = new List<SystemItem>(128);
        try
        {
            var processes = Process.GetProcesses()
                .OrderBy(p => p.ProcessName)
                .ToList();

            items.Add(new SystemItem { Name = "Total Processes", Value = processes.Count.ToString(), CategoryId = "processes" });
            items.Add(new SystemItem { Name = "---", Value = "---", CategoryId = "processes" });

            foreach (var proc in processes)
            {
                try
                {
                    var memMb = proc.WorkingSet64 / (1024.0 * 1024);
                    items.Add(new SystemItem
                    {
                        Name = $"[{proc.Id}] {proc.ProcessName}",
                        Value = $"{memMb:F1} MB | Threads: {proc.Threads.Count}",
                        CategoryId = "processes"
                    });
                }
                catch
                {
                    items.Add(new SystemItem
                    {
                        Name = $"[{proc.Id}] {proc.ProcessName}",
                        Value = "Access Denied",
                        CategoryId = "processes"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            items.Add(new SystemItem { Name = "Error", Value = ex.Message, CategoryId = "processes" });
        }
        return items;
    }

    public List<SystemItem> GetUsbDevices()
    {
        var items = new List<SystemItem>(32);
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Name, Description, Manufacturer, DeviceID, Status " +
                "FROM Win32_PnPEntity WHERE PNPClass='USB' OR Service='usbhub' OR Service='usbhub3'");
            foreach (var obj in searcher.Get())
            {
                using (obj)
                {
                    var name = obj["Name"]?.ToString() ?? "Unknown USB Device";
                    var manufacturer = obj["Manufacturer"]?.ToString() ?? "N/A";
                    var status = obj["Status"]?.ToString() ?? "N/A";
                    items.Add(new SystemItem { Name = name, Value = $"{manufacturer} | Status: {status}", CategoryId = "usb" });
                }
            }

            using var usbSearcher = new ManagementObjectSearcher(
                "SELECT Name, Description, DeviceID, Status FROM Win32_USBHub");
            foreach (var obj in usbSearcher.Get())
            {
                using (obj)
                {
                    var name = obj["Name"]?.ToString() ?? "USB Hub";
                    var desc = obj["Description"]?.ToString() ?? "";
                    if (!items.Any(i => i.Name == name))
                        items.Add(new SystemItem { Name = name, Value = desc, CategoryId = "usb" });
                }
            }

            if (items.Count == 0)
                items.Add(new SystemItem { Name = "Info", Value = "No USB devices detected", CategoryId = "usb" });
        }
        catch (Exception ex)
        {
            items.Add(new SystemItem { Name = "Error", Value = ex.Message, CategoryId = "usb" });
        }
        return items;
    }
}
