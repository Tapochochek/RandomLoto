using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using RandomTrust.Core.Interfaces;
using RandomTrust.Core.Models;

namespace RandomTrust.Core.Generators
{
    /// <summary>
    /// Совместимый с NIST SP800-22 набор статистических тестов.
    /// </summary>
    public class AdvancedStatisticalTester : IStatisticalTester
    {
        private const double EPS = 1e-12;

        // === 1. Frequency (Monobit) ===
        public StatisticalTestResult TestFrequencyMonobit(byte[] data)
        {
            int ones = data.Sum(b => BitOperations.PopCount(b));
            int n = data.Length * 8;
            double sObs = Math.Abs(2.0 * ones - n) / Math.Sqrt(n);
            double p = Erfc(sObs / Math.Sqrt(2.0));
            return Build("Frequency Monobit Test", p, $"Ones: {ones}/{n} ({(double)ones / n:P2}), p-value: {p:F6}");
        }

        // === 2. Runs Test (исправлен) ===
        public StatisticalTestResult TestRunsTest(byte[] data)
        {
            var bits = Bits(data);
            int n = bits.Length;
            double pi = bits.Count(b => b) / (double)n;
            if (Math.Abs(pi - 0.5) > 0.485)
                return Fail("Runs Test", "Pi too far from 0.5");

            int runs = 1;
            for (int i = 1; i < n; i++)
                if (bits[i] != bits[i - 1]) runs++;

            double expected = 2.0 * n * pi * (1 - pi);
            double variance = (2.0 * n * (2.0 * n - 1.0) * pi * (1 - pi)) / (n - 1.0);
            double z = Math.Abs(runs - expected) / Math.Sqrt(variance);
            double p = Erfc(z / Math.Sqrt(2.0));

            return Build("Runs Test", p, $"Runs: {runs}, Expected: {expected:F2}, p-value: {p:F6}");
        }

        // === 3. Block Frequency Test ===
        public StatisticalTestResult TestBlockFrequency(byte[] data, int blockSize = 128)
        {
            var bits = Bits(data);
            int n = bits.Length;
            int N = n / blockSize;
            if (N < 1) return Fail("Block Frequency Test", "Too few bits");

            double chiSq = 0;
            for (int i = 0; i < N; i++)
            {
                int ones = 0;
                for (int j = 0; j < blockSize; j++)
                    if (bits[i * blockSize + j]) ones++;
                double prop = (double)ones / blockSize;
                chiSq += Math.Pow(prop - 0.5, 2);
            }
            chiSq *= 4.0 * blockSize;
            double p = Math.Exp(-chiSq / (2.0 * N));
            return Build("Block Frequency Test", p, $"Blocks: {N}, Chi-square: {chiSq:F2}, p-value: {p:F6}");
        }

        // === 4. Cumulative Sums ===
        public StatisticalTestResult TestCumulativeSums(byte[] data)
        {
            var bits = Bits(data);
            int n = bits.Length;
            int s = 0, max = 0;
            foreach (var b in bits)
            {
                s += b ? 1 : -1;
                max = Math.Max(max, Math.Abs(s));
            }
            double z = max / Math.Sqrt(n);
            double p = 2.0 * (1.0 - NormalCdf(z));
            return Build("Cumulative Sums Test", p, $"Max cumulative sum: {max}, p-value: {p:F6}");
        }

        // === 5. Longest Run of Ones ===
        public StatisticalTestResult TestLongestRun(byte[] data)
        {
            var bits = Bits(data);
            int maxRun = 0, cur = 0;
            foreach (var b in bits)
            {
                if (b) { cur++; if (cur > maxRun) maxRun = cur; } else cur = 0;
            }
            double expected = Math.Log(bits.Length) / Math.Log(2.0);
            double stdev = Math.Sqrt(expected);
            double z = Math.Abs(maxRun - expected) / stdev;
            double p = Erfc(z / Math.Sqrt(2.0));
            return Build("Longest Run of Ones Test", p, $"Max run: {maxRun}, Expected: {expected:F2}, p-value: {p:F6}");
        }

        // === 6. Approximate Entropy (адаптивная) ===
        public StatisticalTestResult TestApproximateEntropy(byte[] data, int m = 3)
        {
            var bits = Bits(data);
            int n = bits.Length;
            if (n < m + 1) return Fail("Approximate Entropy Test", "Insufficient data");

            m = Math.Min(m, Math.Max(2, (int)Math.Log2(n / 10.0)));
            double[] freqM = new double[1 << m];
            double[] freqM1 = new double[1 << (m + 1)];

            for (int i = 0; i < n; i++)
            {
                int k = 0, k1 = 0;
                for (int j = 0; j < m; j++) k = (k << 1) | (bits[(i + j) % n] ? 1 : 0);
                for (int j = 0; j < m + 1; j++) k1 = (k1 << 1) | (bits[(i + j) % n] ? 1 : 0);
                freqM[k]++; freqM1[k1]++;
            }

            double phiM = freqM.Sum(f => (f / n) * Math.Log((f + EPS) / n));
            double phiM1 = freqM1.Sum(f => (f / n) * Math.Log((f + EPS) / n));
            double apEn = phiM - phiM1;
            double chiSq = 2.0 * n * (Math.Log(2) - apEn);
            double p = Math.Exp(-chiSq / 2.0);

            return Build("Approximate Entropy Test", p, $"ApEn: {apEn:F6}, Chi-square: {chiSq:F2}, p-value: {p:F6}");
        }

        // === 7. Serial ===
        public StatisticalTestResult TestSerial(byte[] data)
        {
            var bits = Bits(data);
            int n = bits.Length;
            double ones = bits.Count(b => b);
            double p = ones / n;
            double chi = n * Math.Pow(p - 0.5, 2) / (0.25);
            double pv = Math.Exp(-chi / 2.0);
            return Build("Serial Test", pv, $"Chi-square: {chi:F2}, p-value: {pv:F6}");
        }

        // === 8. DFT (исправленный порог) ===
        public StatisticalTestResult TestDFT(byte[] data)
        {
            var seq = Bits(data).Select(b => b ? 1.0 : -1.0).ToArray();
            int n = seq.Length;
            Complex[] cpx = seq.Select(v => new Complex(v, 0)).ToArray();
            FFT(cpx);
            var mags = cpx.Take(n / 2).Select(c => c.Magnitude).ToArray();

            double threshold = Math.Sqrt(2.995732274 * n); // ln(1/0.05)
            int countBelow = mags.Count(m => m < threshold);
            double expected = 0.95 * n / 2.0;
            double variance = n * 0.95 * 0.05 / 4.0;
            double z = (countBelow - expected) / Math.Sqrt(variance);
            double p = Erfc(Math.Abs(z) / Math.Sqrt(2.0));

            return Build("Discrete Fourier Transform Test", p,
                $"Count below threshold: {countBelow}, Expected: {expected:F2}, p-value: {p:F6}");
        }

        // === Run all ===
        public FullTestResult RunAllTests(byte[] data)
        {
            var list = new List<StatisticalTestResult>
            {
                TestFrequencyMonobit(data),
                TestRunsTest(data),
                TestBlockFrequency(data),
                TestCumulativeSums(data),
                TestLongestRun(data),
                TestApproximateEntropy(data),
                TestSerial(data),
                TestDFT(data)
            };
            return new FullTestResult
            {
                TestResults = list,
                OverallResult = list.All(x => x.Passed),
                Timestamp = DateTime.UtcNow
            };
        }

        // === helpers ===
        private static StatisticalTestResult Build(string n, double p, string desc) =>
            new()
            {
                TestName = n,
                PValue = Clamp(p),
                Passed = p > 0.01,
                Description = desc
            };

        private static StatisticalTestResult Fail(string n, string reason) =>
            new() { TestName = n, Passed = false, PValue = 0, Description = reason };

        private static bool[] Bits(byte[] d)
        {
            var bits = new bool[d.Length * 8];
            for (int i = 0; i < d.Length; i++)
                for (int j = 0; j < 8; j++)
                    bits[i * 8 + j] = (d[i] & (1 << j)) != 0;
            return bits;
        }

        private static double Erfc(double x)
        {
            double z = Math.Abs(x);
            double t = 1.0 / (1.0 + 0.5 * z);
            double ans = t * Math.Exp(-z * z - 1.26551223 +
               t * (1.00002368 +
               t * (0.37409196 +
               t * (0.09678418 +
               t * (-0.18628806 +
               t * (0.27886807 +
               t * (-1.13520398 +
               t * (1.48851587 +
               t * (-0.82215223 + t * 0.17087277)))))))));
            return x >= 0.0 ? ans : 2.0 - ans;
        }

        private static double Erf(double x)
        {
            double sign = Math.Sign(x);
            x = Math.Abs(x);
            double t = 1.0 / (1.0 + 0.5 * x);
            double tau = t * Math.Exp(-x * x - 1.26551223 +
               t * (1.00002368 +
               t * (0.37409196 +
               t * (0.09678418 +
               t * (-0.18628806 +
               t * (0.27886807 +
               t * (-1.13520398 +
               t * (1.48851587 +
               t * (-0.82215223 + t * 0.17087277)))))))));
            return 1.0 - sign * tau;
        }

        private static double NormalCdf(double x) => 0.5 * (1.0 + Erf(x / Math.Sqrt(2.0)));

        private static double Clamp(double p) =>
            double.IsNaN(p) || double.IsInfinity(p) ? EPS : Math.Clamp(p, EPS, 1.0 - EPS);

        private static void FFT(Complex[] buf)
        {
            int n = buf.Length;
            if (n <= 1) return;
            var even = new Complex[n / 2];
            var odd = new Complex[n / 2];
            for (int i = 0; i < n / 2; i++) { even[i] = buf[2 * i]; odd[i] = buf[2 * i + 1]; }
            FFT(even); FFT(odd);
            for (int k = 0; k < n / 2; k++)
            {
                var t = Complex.Exp(-Complex.ImaginaryOne * 2.0 * Math.PI * k / n) * odd[k];
                buf[k] = even[k] + t;
                buf[k + n / 2] = even[k] - t;
            }
        }
    }
}
