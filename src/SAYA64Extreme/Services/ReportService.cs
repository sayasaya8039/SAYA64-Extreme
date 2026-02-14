using System.IO;
using SAYA64Extreme.Models;
using System.Text;

namespace SAYA64Extreme.Services;

public class ReportService
{
    private readonly WmiService _wmiService;

    public ReportService(WmiService wmiService)
    {
        _wmiService = wmiService;
    }

    public string GenerateHtmlReport()
    {
        var sb = new StringBuilder(8192);
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head>");
        sb.AppendLine("<meta charset='utf-8'>");
        sb.AppendLine("<title>SAYA64 Extreme - System Report</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: 'Segoe UI', Tahoma, sans-serif; background: #1e1e2e; color: #cdd6f4; padding: 20px; }");
        sb.AppendLine("h1 { color: #7aa2f7; border-bottom: 2px solid #45475a; padding-bottom: 10px; }");
        sb.AppendLine("h2 { color: #89b4fa; margin-top: 30px; }");
        sb.AppendLine("table { border-collapse: collapse; width: 100%; margin-bottom: 20px; }");
        sb.AppendLine("th { background: #181825; color: #7aa2f7; text-align: left; padding: 8px 12px; border: 1px solid #45475a; }");
        sb.AppendLine("td { padding: 6px 12px; border: 1px solid #45475a; }");
        sb.AppendLine("tr:nth-child(even) { background: #2b2b3d; }");
        sb.AppendLine(".footer { margin-top: 40px; color: #6c7086; font-size: 12px; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine("<h1>SAYA64 Extreme - System Report</h1>");
        sb.AppendLine($"<p>Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");

        AppendSection(sb, "Computer Summary", _wmiService.GetComputerSummary());
        AppendSection(sb, "CPU", _wmiService.GetCpuInfo());
        AppendSection(sb, "Motherboard", _wmiService.GetMotherboardInfo());
        AppendSection(sb, "Memory", _wmiService.GetMemoryInfo());
        AppendSection(sb, "Operating System", _wmiService.GetOsInfo());

        try { AppendSection(sb, "Storage", _wmiService.GetStorageInfo()); } catch { }
        try { AppendSection(sb, "GPU", _wmiService.GetGpuInfo()); } catch { }
        try { AppendSection(sb, "Network", _wmiService.GetNetworkInfo()); } catch { }

        sb.AppendLine("<div class='footer'>");
        sb.AppendLine("<p>SAYA64 Extreme v2.0.0 - System Information &amp; Diagnostic Tool</p>");
        sb.AppendLine("</div>");
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    public string ExportToFile(string? directory = null)
    {
        directory ??= Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var fileName = $"SAYA64_Report_{DateTime.Now:yyyyMMdd_HHmmss}.html";
        var filePath = Path.Combine(directory, fileName);

        var html = GenerateHtmlReport();
        File.WriteAllText(filePath, html, Encoding.UTF8);
        return filePath;
    }

    private static void AppendSection(StringBuilder sb, string title, List<SystemItem> items)
    {
        sb.AppendLine($"<h2>{System.Net.WebUtility.HtmlEncode(title)}</h2>");
        sb.AppendLine("<table><tr><th>Property</th><th>Value</th></tr>");
        foreach (var item in items)
        {
            var name = System.Net.WebUtility.HtmlEncode(item.Name);
            var value = System.Net.WebUtility.HtmlEncode(item.Value);
            sb.AppendLine($"<tr><td>{name}</td><td>{value}</td></tr>");
        }
        sb.AppendLine("</table>");
    }
}
