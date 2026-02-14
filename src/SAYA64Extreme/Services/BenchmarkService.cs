using System.Diagnostics;
using System.Runtime.InteropServices;
using SAYA64Extreme.Models;

namespace SAYA64Extreme.Services;

public class BenchmarkService
{
    private const int DefaultBlockSize = 64 * 1024 * 1024; // 64MB
    private const int Iterations = 5;

    public List<SystemItem> RunMemoryBenchmark()
    {
        var items = new List<SystemItem>(16);
        items.Add(new SystemItem { Name = "Memory Benchmark", Value = "Running...", CategoryId = "bench-memory" });

        try
        {
            var writeSpeed = MeasureWrite(DefaultBlockSize, Iterations);
            var readSpeed = MeasureRead(DefaultBlockSize, Iterations);
            var copySpeed = MeasureCopy(DefaultBlockSize, Iterations);
            var latency = MeasureLatency();

            items.Clear();
            items.Add(new SystemItem { Name = "Block Size", Value = $"{DefaultBlockSize / (1024 * 1024)} MB", CategoryId = "bench-memory" });
            items.Add(new SystemItem { Name = "Iterations", Value = Iterations.ToString(), CategoryId = "bench-memory" });
            items.Add(new SystemItem { Name = "---", Value = "---", CategoryId = "bench-memory" });
            items.Add(new SystemItem { Name = "Memory Write", Value = $"{writeSpeed:F0} MB/s", CategoryId = "bench-memory" });
            items.Add(new SystemItem { Name = "Memory Read", Value = $"{readSpeed:F0} MB/s", CategoryId = "bench-memory" });
            items.Add(new SystemItem { Name = "Memory Copy", Value = $"{copySpeed:F0} MB/s", CategoryId = "bench-memory" });
            items.Add(new SystemItem { Name = "Memory Latency", Value = $"{latency:F1} ns", CategoryId = "bench-memory" });
        }
        catch (Exception ex)
        {
            items.Add(new SystemItem { Name = "Error", Value = ex.Message, CategoryId = "bench-memory" });
        }
        return items;
    }

    private static double MeasureWrite(int blockSize, int iterations)
    {
        var data = new byte[blockSize];
        var rng = new Random(42);
        rng.NextBytes(data);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var buffer = new byte[blockSize];
            Buffer.BlockCopy(data, 0, buffer, 0, blockSize);
        }
        sw.Stop();

        var totalMb = (double)blockSize * iterations / (1024 * 1024);
        return totalMb / sw.Elapsed.TotalSeconds;
    }

    private static double MeasureRead(int blockSize, int iterations)
    {
        var data = new byte[blockSize];
        var rng = new Random(42);
        rng.NextBytes(data);
        long sum = 0;

        var sw = Stopwatch.StartNew();
        for (int iter = 0; iter < iterations; iter++)
        {
            for (int i = 0; i < blockSize; i += 64)
            {
                sum += data[i];
            }
        }
        sw.Stop();

        GC.KeepAlive(sum);
        var totalMb = (double)blockSize * iterations / (1024 * 1024);
        return totalMb / sw.Elapsed.TotalSeconds;
    }

    private static double MeasureCopy(int blockSize, int iterations)
    {
        var src = new byte[blockSize];
        var dst = new byte[blockSize];
        var rng = new Random(42);
        rng.NextBytes(src);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            Buffer.BlockCopy(src, 0, dst, 0, blockSize);
        }
        sw.Stop();

        var totalMb = (double)blockSize * iterations / (1024 * 1024);
        return totalMb / sw.Elapsed.TotalSeconds;
    }

    private static double MeasureLatency()
    {
        const int arraySize = 16 * 1024 * 1024;
        var data = new int[arraySize];
        var rng = new Random(42);

        for (int i = 0; i < arraySize; i++)
            data[i] = rng.Next(arraySize);

        int index = 0;
        const int accessCount = 10_000_000;

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < accessCount; i++)
        {
            index = data[index % arraySize];
        }
        sw.Stop();

        GC.KeepAlive(index);
        return sw.Elapsed.TotalNanoseconds / accessCount;
    }
}
