using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using SAYA64Extreme.Models;

namespace SAYA64Extreme.Services;

public class MemoryBenchmarkService
{
    private const int BufferSize = 64 * 1024 * 1024;       // 64MB
    private const int CopySize = 32 * 1024 * 1024;         // 32MB
    private const int LatencyBufferSize = 128 * 1024 * 1024; // 128MB
    private const int WarmupIterations = 2;
    private const int MeasureIterations = 5;

    // ── Read Benchmark ──────────────────────────────────────────────
    public BenchmarkResult RunReadBenchmark()
    {
        int threadCount = Environment.ProcessorCount;
        int perThread = BufferSize / threadCount;
        byte[] pinned = GC.AllocateArray<byte>(BufferSize, pinned: true);

        // fill with data so reads are not optimized away
        var rng = new Random(42);
        rng.NextBytes(pinned);

        // warmup
        RunParallelRead(pinned, perThread, threadCount);

        var sw = Stopwatch.StartNew();
        long totalBytes = 0;
        for (int iter = 0; iter < MeasureIterations; iter++)
        {
            totalBytes += RunParallelRead(pinned, perThread, threadCount);
        }
        sw.Stop();

        double mbPerSec = totalBytes / (1024.0 * 1024.0) / sw.Elapsed.TotalSeconds;

        return new BenchmarkResult
        {
            TestName = "Memory Read",
            Category = "Memory",
            Score = Math.Round(mbPerSec, 1),
            Unit = "MB/s",
            DurationMs = sw.Elapsed.TotalMilliseconds,
            ThreadCount = threadCount,
            SimdInfo = "unsafe pointer sequential read"
        };
    }

    private static unsafe long RunParallelRead(byte[] buffer, int perThread, int threadCount)
    {
        long totalRead = 0;
        fixed (byte* basePtr = buffer)
        {
            byte* bp = basePtr;
            Parallel.For(0, threadCount, tid =>
            {
                long localSum = 0;
                byte* p = bp + (long)tid * perThread;
                byte* end = p + perThread;
                while (p < end)
                {
                    localSum += *(long*)p;
                    p += 64; // cache-line stride
                }
                GC.KeepAlive(localSum);
                Interlocked.Add(ref totalRead, perThread);
            });
        }
        return totalRead;
    }

    // ── Write Benchmark ─────────────────────────────────────────────
    public BenchmarkResult RunWriteBenchmark()
    {
        int threadCount = Environment.ProcessorCount;
        int perThread = BufferSize / threadCount;
        byte[] pinned = GC.AllocateArray<byte>(BufferSize, pinned: true);

        // warmup
        RunParallelWrite(pinned, perThread, threadCount);

        var sw = Stopwatch.StartNew();
        long totalBytes = 0;
        for (int iter = 0; iter < MeasureIterations; iter++)
        {
            totalBytes += RunParallelWrite(pinned, perThread, threadCount);
        }
        sw.Stop();

        double mbPerSec = totalBytes / (1024.0 * 1024.0) / sw.Elapsed.TotalSeconds;

        return new BenchmarkResult
        {
            TestName = "Memory Write",
            Category = "Memory",
            Score = Math.Round(mbPerSec, 1),
            Unit = "MB/s",
            DurationMs = sw.Elapsed.TotalMilliseconds,
            ThreadCount = threadCount,
            SimdInfo = "unsafe pointer sequential write"
        };
    }

    private static unsafe long RunParallelWrite(byte[] buffer, int perThread, int threadCount)
    {
        long totalWritten = 0;
        fixed (byte* basePtr = buffer)
        {
            byte* bp = basePtr;
            Parallel.For(0, threadCount, tid =>
            {
                byte* p = bp + (long)tid * perThread;
                byte* end = p + perThread;
                long val = 0x5A5A5A5A_5A5A5A5A;
                while (p < end)
                {
                    *(long*)p = val;
                    p += 64;
                }
                Interlocked.Add(ref totalWritten, perThread);
            });
        }
        return totalWritten;
    }

    // ── Copy Benchmark ──────────────────────────────────────────────
    public BenchmarkResult RunCopyBenchmark()
    {
        int threadCount = Environment.ProcessorCount;
        int perThread = CopySize / threadCount;
        byte[] src = GC.AllocateArray<byte>(CopySize, pinned: true);
        byte[] dst = GC.AllocateArray<byte>(CopySize, pinned: true);

        var rng = new Random(42);
        rng.NextBytes(src);

        // warmup
        RunParallelCopy(src, dst, perThread, threadCount);

        var sw = Stopwatch.StartNew();
        long totalBytes = 0;
        for (int iter = 0; iter < MeasureIterations; iter++)
        {
            totalBytes += RunParallelCopy(src, dst, perThread, threadCount);
        }
        sw.Stop();

        double mbPerSec = totalBytes / (1024.0 * 1024.0) / sw.Elapsed.TotalSeconds;

        return new BenchmarkResult
        {
            TestName = "Memory Copy",
            Category = "Memory",
            Score = Math.Round(mbPerSec, 1),
            Unit = "MB/s",
            DurationMs = sw.Elapsed.TotalMilliseconds,
            ThreadCount = threadCount,
            SimdInfo = "Buffer.MemoryCopy parallel"
        };
    }

    private static unsafe long RunParallelCopy(byte[] src, byte[] dst, int perThread, int threadCount)
    {
        long totalCopied = 0;
        fixed (byte* srcBase = src)
        fixed (byte* dstBase = dst)
        {
            byte* sb = srcBase;
            byte* db = dstBase;
            Parallel.For(0, threadCount, tid =>
            {
                long offset = (long)tid * perThread;
                Buffer.MemoryCopy(sb + offset, db + offset, perThread, perThread);
                Interlocked.Add(ref totalCopied, perThread);
            });
        }
        return totalCopied;
    }

    // ── Latency Benchmark ───────────────────────────────────────────
    public BenchmarkResult RunLatencyBenchmark()
    {
        int elementCount = LatencyBufferSize / sizeof(int);
        int[] chain = GC.AllocateArray<int>(elementCount, pinned: true);

        // Build pointer-chase chain (random permutation)
        BuildRandomChain(chain, elementCount);

        const int chaseSteps = 20_000_000;

        // warmup
        ChasePointers(chain, elementCount, chaseSteps / 10);

        var sw = Stopwatch.StartNew();
        int sink = ChasePointers(chain, elementCount, chaseSteps);
        sw.Stop();

        GC.KeepAlive(sink);

        double nsPerAccess = sw.Elapsed.TotalNanoseconds / chaseSteps;

        return new BenchmarkResult
        {
            TestName = "Memory Latency",
            Category = "Memory",
            Score = Math.Round(nsPerAccess, 2),
            Unit = "ns",
            DurationMs = sw.Elapsed.TotalMilliseconds,
            ThreadCount = 1,
            SimdInfo = "pointer-chain random access 128MB"
        };
    }

    private static void BuildRandomChain(int[] chain, int count)
    {
        // Fisher-Yates shuffle to create a single-cycle random permutation
        for (int i = 0; i < count; i++)
            chain[i] = i;

        var rng = new Random(12345);
        for (int i = count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (chain[i], chain[j]) = (chain[j], chain[i]);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int ChasePointers(int[] chain, int count, int steps)
    {
        int index = 0;
        fixed (int* p = chain)
        {
            for (int i = 0; i < steps; i++)
            {
                index = p[index];
            }
        }
        return index;
    }

    // ── Run All ─────────────────────────────────────────────────────
    public List<BenchmarkResult> RunAll()
    {
        return new List<BenchmarkResult>
        {
            RunReadBenchmark(),
            RunWriteBenchmark(),
            RunCopyBenchmark(),
            RunLatencyBenchmark()
        };
    }
}
