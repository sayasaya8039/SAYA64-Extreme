using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAYA64Extreme.Models;
using SAYA64Extreme.Services;

namespace SAYA64Extreme.ViewModels;

public partial class BenchmarkViewModel : ObservableObject
{
    private readonly MemoryBenchmarkService _memoryBench = new();
    private readonly CpuBenchmarkService _cpuBench = new();
    private readonly FpuBenchmarkService _fpuBench = new();

    [ObservableProperty]
    private ObservableCollection<BenchmarkResult> benchmarkResults = new();

    [ObservableProperty]
    private bool isRunning;

    [ObservableProperty]
    private string benchmarkStatus = "Ready";

    public event Action? BenchmarkCompleted;

    [RelayCommand(CanExecute = nameof(CanRunBenchmark))]
    private async Task RunMemoryBenchmark()
    {
        IsRunning = true;
        BenchmarkStatus = "Running memory benchmark...";
        try
        {
            var results = await Task.Run(() => _memoryBench.RunAll());
            BenchmarkResults = new ObservableCollection<BenchmarkResult>(results);
            BenchmarkStatus = $"Memory benchmark complete - {results.Count} results";
        }
        catch (Exception ex)
        {
            BenchmarkStatus = $"Benchmark failed: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
        }
        BenchmarkCompleted?.Invoke();
    }

    [RelayCommand(CanExecute = nameof(CanRunBenchmark))]
    private async Task RunCpuBenchmark()
    {
        IsRunning = true;
        BenchmarkStatus = "Running CPU benchmark...";
        try
        {
            var results = await Task.Run(() => _cpuBench.RunAll());
            foreach (var r in results) BenchmarkResults.Add(r);
            BenchmarkStatus = $"CPU benchmark complete - {results.Count} results";
        }
        catch (Exception ex)
        {
            BenchmarkStatus = $"CPU benchmark failed: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
        }
        BenchmarkCompleted?.Invoke();
    }

    [RelayCommand(CanExecute = nameof(CanRunBenchmark))]
    private async Task RunFpuBenchmark()
    {
        IsRunning = true;
        BenchmarkStatus = "Running FPU benchmark...";
        try
        {
            var results = await Task.Run(() => _fpuBench.RunAll());
            foreach (var r in results) BenchmarkResults.Add(r);
            BenchmarkStatus = $"FPU benchmark complete - {results.Count} results";
        }
        catch (Exception ex)
        {
            BenchmarkStatus = $"FPU benchmark failed: {ex.Message}";
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
        RunCpuBenchmarkCommand.NotifyCanExecuteChanged();
        RunFpuBenchmarkCommand.NotifyCanExecuteChanged();
    }

    public List<SystemItem> GetResults()
    {
        if (BenchmarkResults.Count == 0)
        {
            return new List<SystemItem>
            {
                new() { Name = "Benchmark", Value = "Not yet run", CategoryId = "benchmark" }
            };
        }
        return BenchmarkResults.Select(r => new SystemItem
        {
            Name = r.TestName,
            Value = $"{r.Score} {r.Unit}",
            CategoryId = r.Category
        }).ToList();
    }
}
