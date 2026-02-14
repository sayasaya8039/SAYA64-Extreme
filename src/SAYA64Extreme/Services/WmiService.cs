using System.Management;
using System.Runtime.InteropServices;
using SAYA64Extreme.Models;

namespace SAYA64Extreme.Services;

public class WmiService
{
    public List<SystemItem> GetComputerSummary()
    {
        var items = new List<SystemItem>();
        try
        {
            using var csSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            foreach (var obj in csSearcher.Get())
            {
                items.Add(new SystemItem { Name = "Computer Name", Value = obj["Name"]?.ToString() ?? "N/A", CategoryId = "computer" });
                items.Add(new SystemItem { Name = "Manufacturer", Value = obj["Manufacturer"]?.ToString() ?? "N/A", CategoryId = "computer" });
                items.Add(new SystemItem { Name = "Model", Value = obj["Model"]?.ToString() ?? "N/A", CategoryId = "computer" });
                var totalMemBytes = Convert.ToUInt64(obj["TotalPhysicalMemory"] ?? 0);
                items.Add(new SystemItem { Name = "Total Physical Memory", Value = $"{totalMemBytes / (1024 * 1024 * 1024.0):F1} GB", CategoryId = "computer" });
            }

            using var osSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (var obj in osSearcher.Get())
            {
                items.Add(new SystemItem { Name = "Operating System", Value = obj["Caption"]?.ToString() ?? "N/A", CategoryId = "computer" });
                items.Add(new SystemItem { Name = "OS Version", Value = obj["Version"]?.ToString() ?? "N/A", CategoryId = "computer" });
                items.Add(new SystemItem { Name = "OS Architecture", Value = obj["OSArchitecture"]?.ToString() ?? "N/A", CategoryId = "computer" });
                items.Add(new SystemItem { Name = "Build Number", Value = obj["BuildNumber"]?.ToString() ?? "N/A", CategoryId = "computer" });
            }
        }
        catch (Exception ex)
        {
            items.Add(new SystemItem { Name = "Error", Value = ex.Message, CategoryId = "computer" });
        }
        return items;
    }

    public List<SystemItem> GetCpuInfo()
    {
        var items = new List<SystemItem>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                items.Add(new SystemItem { Name = "Processor Name", Value = obj["Name"]?.ToString()?.Trim() ?? "N/A", CategoryId = "cpu" });
                items.Add(new SystemItem { Name = "Manufacturer", Value = obj["Manufacturer"]?.ToString() ?? "N/A", CategoryId = "cpu" });
                items.Add(new SystemItem { Name = "Max Clock Speed", Value = $"{obj["MaxClockSpeed"]} MHz", CategoryId = "cpu" });
                items.Add(new SystemItem { Name = "Current Clock Speed", Value = $"{obj["CurrentClockSpeed"]} MHz", CategoryId = "cpu" });
                items.Add(new SystemItem { Name = "Number of Cores", Value = obj["NumberOfCores"]?.ToString() ?? "N/A", CategoryId = "cpu" });
                items.Add(new SystemItem { Name = "Number of Logical Processors", Value = obj["NumberOfLogicalProcessors"]?.ToString() ?? "N/A", CategoryId = "cpu" });
                items.Add(new SystemItem { Name = "L2 Cache Size", Value = $"{obj["L2CacheSize"]} KB", CategoryId = "cpu" });
                items.Add(new SystemItem { Name = "L3 Cache Size", Value = $"{obj["L3CacheSize"]} KB", CategoryId = "cpu" });
                items.Add(new SystemItem { Name = "Socket", Value = obj["SocketDesignation"]?.ToString() ?? "N/A", CategoryId = "cpu" });
            }
        }
        catch (Exception ex)
        {
            items.Add(new SystemItem { Name = "Error", Value = ex.Message, CategoryId = "cpu" });
        }
        return items;
    }

    public List<SystemItem> GetMotherboardInfo()
    {
        var items = new List<SystemItem>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            foreach (var obj in searcher.Get())
            {
                items.Add(new SystemItem { Name = "Manufacturer", Value = obj["Manufacturer"]?.ToString() ?? "N/A", CategoryId = "motherboard" });
                items.Add(new SystemItem { Name = "Product", Value = obj["Product"]?.ToString() ?? "N/A", CategoryId = "motherboard" });
                items.Add(new SystemItem { Name = "Serial Number", Value = obj["SerialNumber"]?.ToString() ?? "N/A", CategoryId = "motherboard" });
                items.Add(new SystemItem { Name = "Version", Value = obj["Version"]?.ToString() ?? "N/A", CategoryId = "motherboard" });
            }

            using var biosSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
            foreach (var obj in biosSearcher.Get())
            {
                items.Add(new SystemItem { Name = "BIOS Vendor", Value = obj["Manufacturer"]?.ToString() ?? "N/A", CategoryId = "motherboard" });
                items.Add(new SystemItem { Name = "BIOS Version", Value = obj["SMBIOSBIOSVersion"]?.ToString() ?? "N/A", CategoryId = "motherboard" });
                items.Add(new SystemItem { Name = "BIOS Date", Value = obj["ReleaseDate"]?.ToString() ?? "N/A", CategoryId = "motherboard" });
            }
        }
        catch (Exception ex)
        {
            items.Add(new SystemItem { Name = "Error", Value = ex.Message, CategoryId = "motherboard" });
        }
        return items;
    }

    public List<SystemItem> GetMemoryInfo()
    {
        var items = new List<SystemItem>();
        try
        {
            var memStatus = new NativeMemoryHelper.MEMORYSTATUSEX();
            memStatus.dwLength = (uint)Marshal.SizeOf(typeof(NativeMemoryHelper.MEMORYSTATUSEX));
            if (NativeMemoryHelper.GlobalMemoryStatusEx(ref memStatus))
            {
                items.Add(new SystemItem { Name = "Total Physical Memory", Value = $"{memStatus.ullTotalPhys / (1024.0 * 1024 * 1024):F2} GB", CategoryId = "memory" });
                items.Add(new SystemItem { Name = "Available Physical Memory", Value = $"{memStatus.ullAvailPhys / (1024.0 * 1024 * 1024):F2} GB", CategoryId = "memory" });
                items.Add(new SystemItem { Name = "Memory Load", Value = $"{memStatus.dwMemoryLoad}%", CategoryId = "memory" });
                items.Add(new SystemItem { Name = "Total Virtual Memory", Value = $"{memStatus.ullTotalVirtual / (1024.0 * 1024 * 1024):F2} GB", CategoryId = "memory" });
                items.Add(new SystemItem { Name = "Available Virtual Memory", Value = $"{memStatus.ullAvailVirtual / (1024.0 * 1024 * 1024):F2} GB", CategoryId = "memory" });
            }

            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
            int slotIndex = 0;
            foreach (var obj in searcher.Get())
            {
                var capacity = Convert.ToUInt64(obj["Capacity"] ?? 0);
                items.Add(new SystemItem { Name = $"DIMM Slot #{slotIndex}", Value = $"{capacity / (1024 * 1024 * 1024.0):F0} GB", CategoryId = "memory" });
                items.Add(new SystemItem { Name = $"  Speed", Value = $"{obj["Speed"]} MHz", CategoryId = "memory" });
                items.Add(new SystemItem { Name = $"  Manufacturer", Value = obj["Manufacturer"]?.ToString() ?? "N/A", CategoryId = "memory" });
                items.Add(new SystemItem { Name = $"  Part Number", Value = obj["PartNumber"]?.ToString()?.Trim() ?? "N/A", CategoryId = "memory" });
                slotIndex++;
            }
        }
        catch (Exception ex)
        {
            items.Add(new SystemItem { Name = "Error", Value = ex.Message, CategoryId = "memory" });
        }
        return items;
    }

    public List<SystemItem> GetOsInfo()
    {
        var items = new List<SystemItem>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (var obj in searcher.Get())
            {
                items.Add(new SystemItem { Name = "OS Name", Value = obj["Caption"]?.ToString() ?? "N/A", CategoryId = "os" });
                items.Add(new SystemItem { Name = "Version", Value = obj["Version"]?.ToString() ?? "N/A", CategoryId = "os" });
                items.Add(new SystemItem { Name = "Build Number", Value = obj["BuildNumber"]?.ToString() ?? "N/A", CategoryId = "os" });
                items.Add(new SystemItem { Name = "Architecture", Value = obj["OSArchitecture"]?.ToString() ?? "N/A", CategoryId = "os" });
                items.Add(new SystemItem { Name = "Install Date", Value = ManagementDateTimeConverter.ToDateTime(obj["InstallDate"]?.ToString() ?? "").ToString("yyyy-MM-dd HH:mm:ss"), CategoryId = "os" });
                items.Add(new SystemItem { Name = "Last Boot", Value = ManagementDateTimeConverter.ToDateTime(obj["LastBootUpTime"]?.ToString() ?? "").ToString("yyyy-MM-dd HH:mm:ss"), CategoryId = "os" });
                items.Add(new SystemItem { Name = "System Directory", Value = obj["SystemDirectory"]?.ToString() ?? "N/A", CategoryId = "os" });
                items.Add(new SystemItem { Name = "Windows Directory", Value = obj["WindowsDirectory"]?.ToString() ?? "N/A", CategoryId = "os" });
                items.Add(new SystemItem { Name = "Registered User", Value = obj["RegisteredUser"]?.ToString() ?? "N/A", CategoryId = "os" });
            }

            items.Add(new SystemItem { Name = ".NET Runtime", Value = RuntimeInformation.FrameworkDescription, CategoryId = "os" });
            items.Add(new SystemItem { Name = "Machine Name", Value = Environment.MachineName, CategoryId = "os" });
            items.Add(new SystemItem { Name = "User Name", Value = Environment.UserName, CategoryId = "os" });
        }
        catch (Exception ex)
        {
            items.Add(new SystemItem { Name = "Error", Value = ex.Message, CategoryId = "os" });
        }
        return items;
    }

    // ====== Phase 2: Storage ======

    public List<SystemItem> GetStorageInfo()
    {
        var items = new List<SystemItem>(32);
        try
        {
            using var driveSearcher = new ManagementObjectSearcher(
                "SELECT DeviceID, VolumeName, Size, FreeSpace, FileSystem, DriveType FROM Win32_LogicalDisk WHERE DriveType=3");
            foreach (var obj in driveSearcher.Get())
            {
                using (obj)
                {
                    var deviceId = obj["DeviceID"]?.ToString() ?? "?";
                    var size = Convert.ToUInt64(obj["Size"] ?? 0);
                    var free = Convert.ToUInt64(obj["FreeSpace"] ?? 0);
                    var used = size - free;
                    var usedPercent = size > 0 ? (used * 100.0 / size) : 0;

                    items.Add(new SystemItem { Name = $"Drive {deviceId}", Value = obj["VolumeName"]?.ToString() ?? "Local Disk", CategoryId = "storage" });
                    items.Add(new SystemItem { Name = $"  File System", Value = obj["FileSystem"]?.ToString() ?? "N/A", CategoryId = "storage" });
                    items.Add(new SystemItem { Name = $"  Total Size", Value = $"{size / (1024.0 * 1024 * 1024):F2} GB", CategoryId = "storage" });
                    items.Add(new SystemItem { Name = $"  Free Space", Value = $"{free / (1024.0 * 1024 * 1024):F2} GB", CategoryId = "storage" });
                    items.Add(new SystemItem { Name = $"  Used Space", Value = $"{used / (1024.0 * 1024 * 1024):F2} GB ({usedPercent:F1}%)", CategoryId = "storage" });
                }
            }

            using var diskSearcher = new ManagementObjectSearcher(
                "SELECT Model, InterfaceType, MediaType, Size, SerialNumber, FirmwareRevision FROM Win32_DiskDrive");
            foreach (var obj in diskSearcher.Get())
            {
                using (obj)
                {
                    var model = obj["Model"]?.ToString() ?? "Unknown";
                    var size = Convert.ToUInt64(obj["Size"] ?? 0);
                    items.Add(new SystemItem { Name = "Physical Disk", Value = model, CategoryId = "storage" });
                    items.Add(new SystemItem { Name = "  Interface", Value = obj["InterfaceType"]?.ToString() ?? "N/A", CategoryId = "storage" });
                    items.Add(new SystemItem { Name = "  Media Type", Value = obj["MediaType"]?.ToString() ?? "N/A", CategoryId = "storage" });
                    items.Add(new SystemItem { Name = "  Capacity", Value = $"{size / (1024.0 * 1024 * 1024):F2} GB", CategoryId = "storage" });
                    items.Add(new SystemItem { Name = "  Serial Number", Value = obj["SerialNumber"]?.ToString()?.Trim() ?? "N/A", CategoryId = "storage" });
                    items.Add(new SystemItem { Name = "  Firmware", Value = obj["FirmwareRevision"]?.ToString()?.Trim() ?? "N/A", CategoryId = "storage" });
                }
            }
        }
        catch (Exception ex)
        {
            items.Add(new SystemItem { Name = "Error", Value = ex.Message, CategoryId = "storage" });
        }
        return items;
    }

    public List<SystemItem> GetSmartInfo()
    {
        var items = new List<SystemItem>(16);
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Model, Status, Size, InterfaceType FROM Win32_DiskDrive");
            int index = 0;
            foreach (var obj in searcher.Get())
            {
                using (obj)
                {
                    var model = obj["Model"]?.ToString() ?? "Unknown";
                    var status = obj["Status"]?.ToString() ?? "Unknown";
                    var size = Convert.ToUInt64(obj["Size"] ?? 0);
                    items.Add(new SystemItem { Name = $"Disk #{index}: {model}", Value = "", CategoryId = "smart" });
                    items.Add(new SystemItem { Name = "  Health Status", Value = status, CategoryId = "smart" });
                    items.Add(new SystemItem { Name = "  Capacity", Value = $"{size / (1024.0 * 1024 * 1024):F2} GB", CategoryId = "smart" });
                    items.Add(new SystemItem { Name = "  Interface", Value = obj["InterfaceType"]?.ToString() ?? "N/A", CategoryId = "smart" });
                    index++;
                }
            }

            if (items.Count == 0)
                items.Add(new SystemItem { Name = "Info", Value = "No S.M.A.R.T. data available (run as Administrator)", CategoryId = "smart" });
        }
        catch (Exception ex)
        {
            items.Add(new SystemItem { Name = "Error", Value = ex.Message, CategoryId = "smart" });
        }
        return items;
    }

    // ====== Phase 2: Display ======

    public List<SystemItem> GetGpuInfo()
    {
        var items = new List<SystemItem>(16);
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Name, AdapterRAM, DriverVersion, DriverDate, VideoProcessor, " +
                "CurrentHorizontalResolution, CurrentVerticalResolution, CurrentRefreshRate, " +
                "VideoModeDescription FROM Win32_VideoController");
            int index = 0;
            foreach (var obj in searcher.Get())
            {
                using (obj)
                {
                    var name = obj["Name"]?.ToString() ?? "Unknown GPU";
                    var vram = Convert.ToUInt64(obj["AdapterRAM"] ?? 0);
                    items.Add(new SystemItem { Name = $"GPU #{index}", Value = name, CategoryId = "gpu" });
                    items.Add(new SystemItem { Name = "  Video RAM", Value = vram > 0 ? $"{vram / (1024.0 * 1024 * 1024):F1} GB" : "N/A", CategoryId = "gpu" });
                    items.Add(new SystemItem { Name = "  Driver Version", Value = obj["DriverVersion"]?.ToString() ?? "N/A", CategoryId = "gpu" });
                    items.Add(new SystemItem { Name = "  Video Processor", Value = obj["VideoProcessor"]?.ToString() ?? "N/A", CategoryId = "gpu" });
                    items.Add(new SystemItem { Name = "  Resolution", Value = $"{obj["CurrentHorizontalResolution"]}x{obj["CurrentVerticalResolution"]}", CategoryId = "gpu" });
                    items.Add(new SystemItem { Name = "  Refresh Rate", Value = $"{obj["CurrentRefreshRate"]} Hz", CategoryId = "gpu" });
                    items.Add(new SystemItem { Name = "  Video Mode", Value = obj["VideoModeDescription"]?.ToString() ?? "N/A", CategoryId = "gpu" });
                    index++;
                }
            }
        }
        catch (Exception ex)
        {
            items.Add(new SystemItem { Name = "Error", Value = ex.Message, CategoryId = "gpu" });
        }
        return items;
    }

    public List<SystemItem> GetMonitorInfo()
    {
        var items = new List<SystemItem>(8);
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Name, MonitorManufacturer, MonitorType, ScreenHeight, ScreenWidth " +
                "FROM Win32_DesktopMonitor");
            int index = 0;
            foreach (var obj in searcher.Get())
            {
                using (obj)
                {
                    items.Add(new SystemItem { Name = $"Monitor #{index}", Value = obj["Name"]?.ToString() ?? "Unknown", CategoryId = "monitor" });
                    items.Add(new SystemItem { Name = "  Manufacturer", Value = obj["MonitorManufacturer"]?.ToString() ?? "N/A", CategoryId = "monitor" });
                    items.Add(new SystemItem { Name = "  Type", Value = obj["MonitorType"]?.ToString() ?? "N/A", CategoryId = "monitor" });
                    var w = obj["ScreenWidth"];
                    var h = obj["ScreenHeight"];
                    if (w != null && h != null)
                        items.Add(new SystemItem { Name = "  Native Resolution", Value = $"{w}x{h}", CategoryId = "monitor" });
                    index++;
                }
            }

            try
            {
                var hdc = GetDC(IntPtr.Zero);
                if (hdc != IntPtr.Zero)
                {
                    var dpiX = GetDeviceCaps(hdc, 88); // LOGPIXELSX
                    var dpiY = GetDeviceCaps(hdc, 90); // LOGPIXELSY
                    ReleaseDC(IntPtr.Zero, hdc);
                    items.Add(new SystemItem { Name = "System DPI", Value = $"{dpiX} x {dpiY}", CategoryId = "monitor" });
                }
            }
            catch { items.Add(new SystemItem { Name = "System DPI", Value = "96 (default)", CategoryId = "monitor" }); }
            items.Add(new SystemItem { Name = "Primary Screen Width", Value = $"{System.Windows.SystemParameters.PrimaryScreenWidth}", CategoryId = "monitor" });
            items.Add(new SystemItem { Name = "Primary Screen Height", Value = $"{System.Windows.SystemParameters.PrimaryScreenHeight}", CategoryId = "monitor" });
        }
        catch (Exception ex)
        {
            items.Add(new SystemItem { Name = "Error", Value = ex.Message, CategoryId = "monitor" });
        }
        return items;
    }

    // ====== Phase 2: Network ======

    public List<SystemItem> GetNetworkInfo()
    {
        var items = new List<SystemItem>(32);
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Description, MACAddress, IPAddress, IPSubnet, DefaultIPGateway, " +
                "DNSServerSearchOrder, DHCPEnabled FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled=True");
            int index = 0;
            foreach (var obj in searcher.Get())
            {
                using (obj)
                {
                    items.Add(new SystemItem { Name = $"Adapter #{index}", Value = obj["Description"]?.ToString() ?? "Unknown", CategoryId = "network" });
                    items.Add(new SystemItem { Name = "  MAC Address", Value = obj["MACAddress"]?.ToString() ?? "N/A", CategoryId = "network" });
                    items.Add(new SystemItem { Name = "  DHCP Enabled", Value = obj["DHCPEnabled"]?.ToString() ?? "N/A", CategoryId = "network" });

                    if (obj["IPAddress"] is string[] ips && ips.Length > 0)
                    {
                        for (int i = 0; i < ips.Length; i++)
                            items.Add(new SystemItem { Name = $"  IP Address #{i}", Value = ips[i], CategoryId = "network" });
                    }
                    if (obj["IPSubnet"] is string[] subnets && subnets.Length > 0)
                        items.Add(new SystemItem { Name = "  Subnet Mask", Value = subnets[0], CategoryId = "network" });
                    if (obj["DefaultIPGateway"] is string[] gateways && gateways.Length > 0)
                        items.Add(new SystemItem { Name = "  Default Gateway", Value = gateways[0], CategoryId = "network" });
                    if (obj["DNSServerSearchOrder"] is string[] dns && dns.Length > 0)
                    {
                        for (int i = 0; i < dns.Length; i++)
                            items.Add(new SystemItem { Name = $"  DNS Server #{i}", Value = dns[i], CategoryId = "network" });
                    }
                    index++;
                }
            }

            using var speedSearcher = new ManagementObjectSearcher(
                "SELECT Name, Speed, NetConnectionStatus FROM Win32_NetworkAdapter WHERE NetConnectionStatus=2");
            foreach (var obj in speedSearcher.Get())
            {
                using (obj)
                {
                    var speed = Convert.ToUInt64(obj["Speed"] ?? 0);
                    var speedStr = speed >= 1_000_000_000 ? $"{speed / 1_000_000_000.0:F0} Gbps" :
                                   speed >= 1_000_000 ? $"{speed / 1_000_000.0:F0} Mbps" :
                                   $"{speed} bps";
                    items.Add(new SystemItem { Name = $"Link: {obj["Name"]}", Value = speedStr, CategoryId = "network" });
                }
            }
        }
        catch (Exception ex)
        {
            items.Add(new SystemItem { Name = "Error", Value = ex.Message, CategoryId = "network" });
        }
        return items;
    }

    // P/Invoke for DPI detection
    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
}

public static class NativeMemoryHelper
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);
}
