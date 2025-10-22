using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using RandomTrust.Core.Interfaces;

namespace RandomTrust.Core.EntropySources
{
    /// <summary>
    /// Комбинированный источник энтропии:
    /// пользовательские данные + системные шумы + хаотическое усиление +
    /// CPU jitter + memory latency + thread scheduler jitter.
    /// </summary>
    public class HybridEntropySource : IEntropySource
    {
        public string Name => "Hybrid Entropy Source (User + System + Chaotic + Hardware)";
        private readonly List<double> _entropyPool = new();
        private readonly object _lock = new();
        private const double ChaosParameter = 3.99;
        private readonly Stopwatch _uptime = Stopwatch.StartNew();

        public double CurrentEntropyLevel => Math.Min(100.0, _entropyPool.Count / 150.0 * 100.0);
        public event Action<double>? OnEntropyProgress;

        public void AddUserEntropy(params double[] data)
        {
            lock (_lock)
            {
                foreach (var d in data)
                {
                    double mixed = d
                        + GetSystemNoise()
                        + GetCpuJitter()
                        + GetMemoryJitter()
                        + GetThreadSchedulerJitter();

                    _entropyPool.Add(mixed % 1.0);
                }

                if (_entropyPool.Count > 1000)
                    _entropyPool.RemoveRange(0, _entropyPool.Count - 800);

                OnEntropyProgress?.Invoke(CurrentEntropyLevel);
                Console.WriteLine($"Added entropy point. Current level: {CurrentEntropyLevel:F2}");
            }
        }

        public byte[] GenerateSeed()
        {
            lock (_lock)
            {
                if (_entropyPool.Count < 10)
                    throw new InvalidOperationException($"Недостаточно энтропии ({_entropyPool.Count}/10).");

                Console.WriteLine($"Mixing {_entropyPool.Count} entropy samples...");

                double combined = CombineEntropy();
                double chaotic = ApplyLogisticMap(combined, 2000);

                byte[] noise = RandomNumberGenerator.GetBytes(64);
                byte[] chaoticBytes = BitConverter.GetBytes(chaotic);
                byte[] sysBytes = GetSystemEntropyBytes();

                byte[] merged = chaoticBytes
                    .Concat(noise)
                    .Concat(sysBytes)
                    .Concat(BitConverter.GetBytes(_uptime.ElapsedTicks))
                    .ToArray();

                using var sha512 = SHA512.Create();
                byte[] hash512 = sha512.ComputeHash(merged);

                for (int i = 0; i < chaoticBytes.Length; i++)
                    hash512[i % hash512.Length] ^= chaoticBytes[i % chaoticBytes.Length];

                using var sha256 = SHA256.Create();
                byte[] finalSeed = sha256.ComputeHash(hash512);

                Console.WriteLine($"Final 256bit seed: {Convert.ToBase64String(finalSeed)[..24]}...");
                _entropyPool.Clear();
                OnEntropyProgress?.Invoke(0);
                return finalSeed;
            }
        }

        private double CombineEntropy()
        {
            double sum = 0;
            for (int i = 0; i < _entropyPool.Count; i++)
                sum += _entropyPool[i] * Math.Sin(i + 1) * Math.Cos(i * 0.37);

            return Math.Abs(sum % 1.0);
        }

        private double ApplyLogisticMap(double x, int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                x = ChaosParameter * x * (1 - x);
                x = (x + GetSystemNoise()) % 1.0;
            }
            return x;
        }

        // 🔹 Новый источник 1 — CPU jitter (микрофлуктуации времени выполнения)
        private static double GetCpuJitter()
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
                _ = Math.Sqrt(i * 123.4567);
            sw.Stop();
            return (sw.ElapsedTicks % 1000) / 1000.0;
        }

        // 🔹 Новый источник 2 — Memory latency jitter
        private static double GetMemoryJitter()
        {
            var data = new byte[256];
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < data.Length; i++)
                data[i] ^= (byte)(i * 37);
            sw.Stop();
            return (sw.ElapsedTicks % 1000) / 1000.0;
        }

        // 🔹 Новый источник 3 — Thread scheduler jitter
        private static double GetThreadSchedulerJitter()
        {
            var sw = Stopwatch.StartNew();
            Task.Delay(0).Wait();
            sw.Stop();
            return (sw.ElapsedTicks % 2000) / 2000.0;
        }

        private static double GetSystemNoise()
        {
            double jitter = (DateTime.UtcNow.Ticks % 1000) / 1000.0;
            double rnd = Random.Shared.NextDouble();
            double temp = Math.Sin(Environment.TickCount64 * 0.001);
            return (jitter + rnd + temp) % 1.0;
        }

        private static byte[] GetSystemEntropyBytes()
        {
            using var sha = SHA256.Create();
            var sys = $"{Environment.MachineName}-{Guid.NewGuid()}-{Environment.TickCount64}-{GC.GetTotalMemory(false)}";
            return sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(sys));
        }

        public void Reset()
        {
            lock (_lock)
            {
                _entropyPool.Clear();
                OnEntropyProgress?.Invoke(0);
                Console.WriteLine("Entropy source reset.");
            }
        }
    }
}
