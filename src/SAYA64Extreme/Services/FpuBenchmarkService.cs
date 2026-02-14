using System.Diagnostics;
using System.Runtime.CompilerServices;
using SAYA64Extreme.Models;

namespace SAYA64Extreme.Services;

public class FpuBenchmarkService
{
    // ── Julia Set Benchmark (float) ─────────────────────────────────
    public BenchmarkResult RunJuliaBenchmark()
    {
        const int width = 1024;
        const int height = 1024;
        const int maxIter = 256;
        const float cRe = -0.7f;
        const float cIm = 0.27015f;
        int threadCount = Environment.ProcessorCount;

        int[] output = new int[width * height];

        // warmup
        ComputeJulia(output, width, height, maxIter, cRe, cIm);

        var sw = Stopwatch.StartNew();
        const int iterations = 5;
        for (int i = 0; i < iterations; i++)
        {
            ComputeJulia(output, width, height, maxIter, cRe, cIm);
        }
        sw.Stop();

        double totalMPixels = (double)width * height * iterations / 1_000_000.0;
        double mPixelPerSec = totalMPixels / sw.Elapsed.TotalSeconds;

        return new BenchmarkResult
        {
            TestName = "Julia Set (float)",
            Category = "FPU",
            Score = Math.Round(mPixelPerSec, 2),
            Unit = "MPixel/s",
            DurationMs = sw.Elapsed.TotalMilliseconds,
            ThreadCount = threadCount,
            SimdInfo = $"C=({cRe},{cIm}), {width}x{height}, maxIter={maxIter}, float32"
        };
    }

    private static void ComputeJulia(int[] output, int width, int height, int maxIter, float cRe, float cIm)
    {
        float xMin = -1.5f, xMax = 1.5f;
        float yMin = -1.5f, yMax = 1.5f;
        float xScale = (xMax - xMin) / width;
        float yScale = (yMax - yMin) / height;

        Parallel.For(0, height, y =>
        {
            float zy = yMin + y * yScale;
            int rowOffset = y * width;

            for (int x = 0; x < width; x++)
            {
                float zx = xMin + x * xScale;
                float zxT = zx;
                float zyT = zy;
                int iter = 0;

                while (zxT * zxT + zyT * zyT < 4.0f && iter < maxIter)
                {
                    float tmp = zxT * zxT - zyT * zyT + cRe;
                    zyT = 2.0f * zxT * zyT + cIm;
                    zxT = tmp;
                    iter++;
                }

                output[rowOffset + x] = iter;
            }
        });
    }

    // ── Mandelbrot Set Benchmark (double) ───────────────────────────
    public BenchmarkResult RunMandelBenchmark()
    {
        const int width = 1024;
        const int height = 1024;
        const int maxIter = 256;
        int threadCount = Environment.ProcessorCount;

        int[] output = new int[width * height];

        // warmup
        ComputeMandelbrot(output, width, height, maxIter);

        var sw = Stopwatch.StartNew();
        const int iterations = 5;
        for (int i = 0; i < iterations; i++)
        {
            ComputeMandelbrot(output, width, height, maxIter);
        }
        sw.Stop();

        double totalMPixels = (double)width * height * iterations / 1_000_000.0;
        double mPixelPerSec = totalMPixels / sw.Elapsed.TotalSeconds;

        return new BenchmarkResult
        {
            TestName = "Mandelbrot Set (double)",
            Category = "FPU",
            Score = Math.Round(mPixelPerSec, 2),
            Unit = "MPixel/s",
            DurationMs = sw.Elapsed.TotalMilliseconds,
            ThreadCount = threadCount,
            SimdInfo = $"{width}x{height}, maxIter={maxIter}, float64"
        };
    }

    private static void ComputeMandelbrot(int[] output, int width, int height, int maxIter)
    {
        double xMin = -2.0, xMax = 1.0;
        double yMin = -1.5, yMax = 1.5;
        double xScale = (xMax - xMin) / width;
        double yScale = (yMax - yMin) / height;

        Parallel.For(0, height, y =>
        {
            double ci = yMin + y * yScale;
            int rowOffset = y * width;

            for (int x = 0; x < width; x++)
            {
                double cr = xMin + x * xScale;
                double zr = 0.0, zi = 0.0;
                int iter = 0;

                while (zr * zr + zi * zi < 4.0 && iter < maxIter)
                {
                    double tmp = zr * zr - zi * zi + cr;
                    zi = 2.0 * zr * zi + ci;
                    zr = tmp;
                    iter++;
                }

                output[rowOffset + x] = iter;
            }
        });
    }

    // ── Sin(z) Modified Julia Benchmark ─────────────────────────────
    public BenchmarkResult RunSinJuliaBenchmark()
    {
        const int width = 512;
        const int height = 512;
        const int maxIter = 128;
        int threadCount = Environment.ProcessorCount;

        int[] output = new int[width * height];

        // warmup
        ComputeSinJulia(output, width, height, maxIter);

        var sw = Stopwatch.StartNew();
        const int iterations = 5;
        for (int i = 0; i < iterations; i++)
        {
            ComputeSinJulia(output, width, height, maxIter);
        }
        sw.Stop();

        double totalMPixels = (double)width * height * iterations / 1_000_000.0;
        double mPixelPerSec = totalMPixels / sw.Elapsed.TotalSeconds;

        return new BenchmarkResult
        {
            TestName = "Sin(z) Julia (trig)",
            Category = "FPU",
            Score = Math.Round(mPixelPerSec, 2),
            Unit = "MPixel/s",
            DurationMs = sw.Elapsed.TotalMilliseconds,
            ThreadCount = threadCount,
            SimdInfo = $"{width}x{height}, maxIter={maxIter}, Math.Sin/Cos/Exp heavy"
        };
    }

    /// <summary>
    /// Computes sin(z) modified Julia set.
    /// f(z) = c * sin(z), where c is a constant and sin(z) for complex z is:
    ///   sin(z) = sin(x)*cosh(y) + i*cos(x)*sinh(y)
    /// Using: sinh(y) = (exp(y) - exp(-y))/2,  cosh(y) = (exp(y) + exp(-y))/2
    /// </summary>
    private static void ComputeSinJulia(int[] output, int width, int height, int maxIter)
    {
        // c = 1.0 + 0.3i (produces interesting fractal patterns)
        const double cRe = 1.0;
        const double cIm = 0.3;
        const double escapeRadius = 50.0;

        double xMin = -Math.PI, xMax = Math.PI;
        double yMin = -Math.PI, yMax = Math.PI;
        double xScale = (xMax - xMin) / width;
        double yScale = (yMax - yMin) / height;

        Parallel.For(0, height, y =>
        {
            double zy = yMin + y * yScale;
            int rowOffset = y * width;

            for (int x = 0; x < width; x++)
            {
                double zx = xMin + x * xScale;
                double zRe = zx;
                double zIm = zy;
                int iter = 0;

                while (iter < maxIter)
                {
                    // sin(z) = sin(Re)*cosh(Im) + i*cos(Re)*sinh(Im)
                    double sinRe = Math.Sin(zRe);
                    double cosRe = Math.Cos(zRe);
                    double expPos = Math.Exp(zIm);
                    double expNeg = Math.Exp(-zIm);
                    double coshIm = (expPos + expNeg) * 0.5;
                    double sinhIm = (expPos - expNeg) * 0.5;

                    // w = sin(z)
                    double wRe = sinRe * coshIm;
                    double wIm = cosRe * sinhIm;

                    // z_new = c * w = (cRe + cIm*i) * (wRe + wIm*i)
                    double newRe = cRe * wRe - cIm * wIm;
                    double newIm = cRe * wIm + cIm * wRe;

                    zRe = newRe;
                    zIm = newIm;

                    if (zRe * zRe + zIm * zIm > escapeRadius * escapeRadius)
                        break;

                    iter++;
                }

                output[rowOffset + x] = iter;
            }
        });
    }

    // ── Run All ─────────────────────────────────────────────────────
    public List<BenchmarkResult> RunAll()
    {
        return new List<BenchmarkResult>
        {
            RunJuliaBenchmark(),
            RunMandelBenchmark(),
            RunSinJuliaBenchmark()
        };
    }
}
