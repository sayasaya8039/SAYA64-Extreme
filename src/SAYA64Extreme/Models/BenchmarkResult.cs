namespace SAYA64Extreme.Models;

public class BenchmarkResult
{
    public string TestName { get; set; } = "";
    public string Category { get; set; } = "";
    public double Score { get; set; }
    public string Unit { get; set; } = "";
    public double DurationMs { get; set; }
    public int ThreadCount { get; set; }
    public string SimdInfo { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
