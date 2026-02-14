using CommunityToolkit.Mvvm.ComponentModel;

namespace SAYA64Extreme.Models;

public partial class SensorReading : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string sensorType = string.Empty;

    [ObservableProperty]
    private string currentValue = "N/A";

    [ObservableProperty]
    private string minValue = "N/A";

    [ObservableProperty]
    private string maxValue = "N/A";

    public string Unit { get; set; } = string.Empty;
    public string HardwareName { get; set; } = string.Empty;

    /// <summary>
    /// DataGrid の {Binding Value} 用。CurrentValue を返し、リアルタイム更新に追従する。
    /// </summary>
    public string Value => CurrentValue;

    partial void OnCurrentValueChanged(string value)
    {
        OnPropertyChanged(nameof(Value));
    }
}
