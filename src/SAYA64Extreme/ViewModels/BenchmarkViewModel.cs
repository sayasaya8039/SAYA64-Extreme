using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAYA64Extreme.Models;
using SAYA64Extreme.Services;

namespace SAYA64Extreme.ViewModels;

/// <summary>
/// ベンチマーク実行UIのViewModel。
/// MainViewModelから利用され、結果をSystemItemリストとして提供する。
/// </summary>
public partial class BenchmarkViewModel : ObservableObject
{
    private readonly BenchmarkService _benchmarkService = new();

    [ObservableProperty]
    private ObservableCollection<SystemItem> benchmarkResults = new();

    [ObservableProperty]
    private bool isRunning;

    [ObservableProperty]
    private string benchmarkStatus = "Ready - Click 'Run' to start memory benchmark";

    /// <summary>
    /// ベンチマーク完了時のコールバック。MainViewModelから購読される。
    /// </summary>
    public event Action? BenchmarkCompleted;

    [RelayCommand(CanExecute = nameof(CanRunBenchmark))]
    private async Task RunMemoryBenchmark()
    {
        IsRunning = true;
        BenchmarkStatus = "Running memory benchmark...";
        BenchmarkResults.Clear();

        try
        {
            var results = await Task.Run(() => _benchmarkService.RunMemoryBenchmark());
            BenchmarkResults = new ObservableCollection<SystemItem>(results);
            BenchmarkStatus = $"Benchmark complete - {results.Count} results";
        }
        catch (Exception ex)
        {
            BenchmarkStatus = $"Benchmark failed: {ex.Message}";
            BenchmarkResults.Add(new SystemItem { Name = "Error", Value = ex.Message, CategoryId = "bench-memory" });
        }
        finally
        {
            IsRunning = false;
        }

        BenchmarkCompleted?.Invoke();
    }

    private bool CanRunBenchmark() => !IsRunning;

    partial void OnIsRunningChanged(bool value)
    {
        RunMemoryBenchmarkCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// 結果をSystemItemリストとして返す（MainViewModelのCurrentItems用）。
    /// </summary>
    public List<SystemItem> GetResults()
    {
        if (BenchmarkResults.Count == 0)
        {
            return new List<SystemItem>
            {
                new() { Name = "Memory Benchmark", Value = "Not yet run", CategoryId = "bench-memory" },
                new() { Name = "---", Value = "---", CategoryId = "bench-memory" },
                new() { Name = "Action", Value = "Use Tools > Memory Benchmark to run", CategoryId = "bench-memory" }
            };
        }
        return new List<SystemItem>(BenchmarkResults);
    }
}
