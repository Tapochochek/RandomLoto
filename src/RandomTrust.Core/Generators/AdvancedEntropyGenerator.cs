using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RandomTrust.Core.Generators
{
    /// <summary>
    /// Мощный гибридный генератор энтропии.
    /// Смешивает пользовательскую, системную и криптографическую энтропию.
    /// </summary>
    public static class AdvancedEntropyGenerator
    {
        private const int HashSize = 64; // 512 бит SHA512
        private static readonly Stopwatch _sw = Stopwatch.StartNew();

        /// <summary>
        /// Генерация байтов энтропии из движений мыши с системными шумами.
        /// Работает стабильно даже при малом количестве событий.
        /// </summary>
        public static byte[] GenerateEntropyFromMouse(IEnumerable<(double x, double y, double velocity, double timestamp)> mouseData)
        {
            var buffer = new List<byte>();

            // 1️⃣ Базовые данные движения мыши
            foreach (var (x, y, velocity, timestamp) in mouseData)
            {
                double jitter = (Random.Shared.NextDouble() - 0.5) * 0.0001;
                double noisyTimestamp = timestamp + jitter + _sw.ElapsedTicks % 13;
                buffer.AddRange(BitConverter.GetBytes(x));
                buffer.AddRange(BitConverter.GetBytes(y));
                buffer.AddRange(BitConverter.GetBytes(velocity));
                buffer.AddRange(BitConverter.GetBytes(noisyTimestamp));
            }

            // 2️⃣ Добавляем системную энтропию
            buffer.AddRange(CollectSystemEntropy());

            // 3️⃣ Криптографическое смешивание SHA512
            byte[] mixed = HashMix(buffer.ToArray());

            // 4️⃣ ChaCha-вдохновлённое перемешивание (усиление)
            mixed = ChaChaWhiten(mixed);

            // 5️⃣ XOR с криптографическим RNG
            var cryptoRand = new byte[mixed.Length];
            RandomNumberGenerator.Fill(cryptoRand);
            for (int i = 0; i < mixed.Length; i++)
                mixed[i] ^= cryptoRand[i];

            // 6️⃣ Финальный микс SHA256 для выравнивания распределения
            return SHA256.HashData(mixed);
        }

        // ===== Сбор системной энтропии =====
        private static byte[] CollectSystemEntropy()
        {
            var sys = new List<byte>();
            sys.AddRange(BitConverter.GetBytes(Environment.TickCount64));
            sys.AddRange(BitConverter.GetBytes(Process.GetCurrentProcess().Id));
            sys.AddRange(BitConverter.GetBytes(GC.GetTotalMemory(false)));
            sys.AddRange(BitConverter.GetBytes(DateTime.UtcNow.Ticks));
            sys.AddRange(BitConverter.GetBytes(_sw.ElapsedTicks));
            sys.AddRange(BitConverter.GetBytes(Random.Shared.NextDouble()));

            // Добавляем немного шума от GUID и RNG
            sys.AddRange(Guid.NewGuid().ToByteArray());
            var rand = new byte[32];
            RandomNumberGenerator.Fill(rand);
            sys.AddRange(rand);

            return sys.ToArray();
        }

        // ===== Хэш-миксер =====
        private static byte[] HashMix(byte[] data)
        {
            using var sha = SHA512.Create();
            var hash = sha.ComputeHash(data);
            var extended = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                extended[i] = (byte)(data[i] ^ hash[i % hash.Length]);
            return extended;
        }

        // ===== Псевдо-хаотическое перемешивание (ChaCha-подобное) =====
        private static byte[] ChaChaWhiten(byte[] input)
        {
            byte[] data = (byte[])input.Clone();
            for (int i = 0; i < data.Length - 4; i += 4)
            {
                byte a = data[i];
                byte b = data[i + 1];
                byte c = data[i + 2];
                byte d = data[i + 3];

                byte rot = (byte)((a << 1) | (a >> 7));
                data[i] = (byte)(a ^ b);
                data[i + 1] = (byte)(b + rot);
                data[i + 2] = (byte)(c ^ d ^ rot);
                data[i + 3] = (byte)((d + a + c) & 0xFF);
            }
            return data;
        }
    }
}
