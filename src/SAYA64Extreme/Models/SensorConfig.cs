namespace SAYA64Extreme.Models;

public class SensorCorrection
{
    public string SensorId { get; set; } = "";
    public double Offset { get; set; }
    public double Ratio { get; set; } = 1.0;
}

public class SensorAlert
{
    public string SensorId { get; set; } = "";
    public string SensorName { get; set; } = "";
    public double? MinThreshold { get; set; }
    public double? MaxThreshold { get; set; }
    public AlertAction Action { get; set; } = AlertAction.Warning;
    public bool IsEnabled { get; set; } = true;
}

public enum AlertAction { Warning, Shutdown, Log }

public class SensorPollingConfig
{
    public int IntervalMs { get; set; } = 2000;
    public bool ShareToMemory { get; set; }
    public bool ShareToRegistry { get; set; }
}
