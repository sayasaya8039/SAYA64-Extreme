using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using SAYA64Extreme.Models;

namespace SAYA64Extreme.Services;

public class CpuBenchmarkService
{
    // ── N-Queens Benchmark ──────────────────────────────────────────
    public BenchmarkResult RunQueenBenchmark()
    {
        const int N = 12;
        int threadCount = Environment.ProcessorCount;

        // warmup
        SolveNQueensParallel(N);

        var sw = Stopwatch.StartNew();
        long solutions = SolveNQueensParallel(N);
        sw.Stop();

        return new BenchmarkResult
        {
            TestName = "N-Queens (N=12)",
            Category = "CPU Integer",
            Score = solutions,
            Unit = "solutions",
            DurationMs = sw.Elapsed.TotalMilliseconds,
            ThreadCount = threadCount,
            SimdInfo = $"Parallel.For, {solutions} solutions found"
        };
    }

    private static long SolveNQueensParallel(int n)
    {
        long totalSolutions = 0;

        // Parallelize over first row placements
        Parallel.For(0, n, col =>
        {
            long localCount = 0;
            int[] queens = new int[n];
            queens[0] = col;
            SolveRecursive(queens, 1, n, ref localCount);
            Interlocked.Add(ref totalSolutions, localCount);
        });

        return totalSolutions;
    }

    private static void SolveRecursive(int[] queens, int row, int n, ref long count)
    {
        if (row == n)
        {
            count++;
            return;
        }

        for (int col = 0; col < n; col++)
        {
            bool safe = true;
            for (int prev = 0; prev < row; prev++)
            {
                int diff = row - prev;
                if (queens[prev] == col ||
                    queens[prev] - diff == col ||
                    queens[prev] + diff == col)
                {
                    safe = false;
                    break;
                }
            }

            if (safe)
            {
                queens[row] = col;
                SolveRecursive(queens, row + 1, n, ref count);
            }
        }
    }

    // ── ZLib Compression Benchmark ──────────────────────────────────
    public BenchmarkResult RunZLibBenchmark()
    {
        const int dataSize = 16 * 1024 * 1024; // 16MB
        int threadCount = Environment.ProcessorCount;

        byte[] data = new byte[dataSize];
        var rng = new Random(42);
        // Mix random and repeating data for realistic compression
        for (int i = 0; i < dataSize; i++)
        {
            data[i] = (byte)(i % 256 ^ rng.Next(64));
        }

        // warmup
        CompressDeflate(data);

        var sw = Stopwatch.StartNew();
        long totalCompressed = 0;
        const int iterations = 3;
        for (int i = 0; i < iterations; i++)
        {
            totalCompressed += CompressDeflate(data);
        }
        sw.Stop();

        double totalMb = (double)dataSize * iterations / (1024.0 * 1024.0);
        double mbPerSec = totalMb / sw.Elapsed.TotalSeconds;

        return new BenchmarkResult
        {
            TestName = "ZLib Compression",
            Category = "CPU",
            Score = Math.Round(mbPerSec, 1),
            Unit = "MB/s",
            DurationMs = sw.Elapsed.TotalMilliseconds,
            ThreadCount = 1,
            SimdInfo = "DeflateStream optimal compression"
        };
    }

    private static long CompressDeflate(byte[] data)
    {
        using var ms = new MemoryStream();
        using (var deflate = new DeflateStream(ms, CompressionLevel.Optimal, leaveOpen: true))
        {
            deflate.Write(data, 0, data.Length);
        }
        return ms.Length;
    }

    // ── AES-256-CBC Benchmark ───────────────────────────────────────
    public BenchmarkResult RunAesBenchmark()
    {
        const int dataSize = 64 * 1024 * 1024; // 64MB
        int threadCount = Environment.ProcessorCount;

        byte[] plaintext = new byte[dataSize];
        var rng = new Random(42);
        rng.NextBytes(plaintext);

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateKey();
        aes.GenerateIV();

        byte[] key = aes.Key;
        byte[] iv = aes.IV;

        // warmup
        EncryptAes(plaintext, key, iv);

        var sw = Stopwatch.StartNew();
        const int iterations = 3;
        for (int i = 0; i < iterations; i++)
        {
            EncryptAes(plaintext, key, iv);
        }
        sw.Stop();

        double totalMb = (double)dataSize * iterations / (1024.0 * 1024.0);
        double mbPerSec = totalMb / sw.Elapsed.TotalSeconds;

        return new BenchmarkResult
        {
            TestName = "AES-256-CBC Encrypt",
            Category = "CPU Crypto",
            Score = Math.Round(mbPerSec, 1),
            Unit = "MB/s",
            DurationMs = sw.Elapsed.TotalMilliseconds,
            ThreadCount = 1,
            SimdInfo = "System.Security.Cryptography.Aes (AES-NI)"
        };
    }

    private static byte[] EncryptAes(byte[] data, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;

        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }

    // ── SHA256 Hash Benchmark ───────────────────────────────────────
    public BenchmarkResult RunHashBenchmark()
    {
        const int dataSize = 64 * 1024 * 1024; // 64MB

        byte[] data = new byte[dataSize];
        var rng = new Random(42);
        rng.NextBytes(data);

        using var sha256 = SHA256.Create();

        // warmup
        sha256.ComputeHash(data);

        var sw = Stopwatch.StartNew();
        const int iterations = 5;
        for (int i = 0; i < iterations; i++)
        {
            sha256.ComputeHash(data);
        }
        sw.Stop();

        double totalMb = (double)dataSize * iterations / (1024.0 * 1024.0);
        double mbPerSec = totalMb / sw.Elapsed.TotalSeconds;

        return new BenchmarkResult
        {
            TestName = "SHA-256 Hash",
            Category = "CPU Crypto",
            Score = Math.Round(mbPerSec, 1),
            Unit = "MB/s",
            DurationMs = sw.Elapsed.TotalMilliseconds,
            ThreadCount = 1,
            SimdInfo = "SHA256.Create() (SHA-NI if available)"
        };
    }

    // ── Run All ─────────────────────────────────────────────────────
    public List<BenchmarkResult> RunAll()
    {
        return new List<BenchmarkResult>
        {
            RunQueenBenchmark(),
            RunZLibBenchmark(),
            RunAesBenchmark(),
            RunHashBenchmark()
        };
    }
}
