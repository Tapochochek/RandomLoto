using System;
using System.Linq;
using System.Security.Cryptography;
using RandomTrust.Core.Interfaces;
using RandomTrust.Core.Models;

namespace RandomTrust.Core.Generators
{
    /// <summary>
    /// Реализация HMAC-DRBG (NIST SP 800-90A) с HMAC-SHA256
    /// </summary>
    public class TransparentRNG : IRandomGenerator, IDisposable
    {
        private byte[] _K;
        private byte[] _V;
        private readonly object _sync = new();

        public event Action<GenerationStage>? OnGenerationStage;

        //  Конструктор без параметров — для тестов
        public TransparentRNG()
        {
            _K = Enumerable.Repeat((byte)0x00, 32).ToArray();
            _V = Enumerable.Repeat((byte)0x01, 32).ToArray();
        }

        //  Конструктор с источником энтропии — для DI
        private readonly IEntropySource? _entropySource;
        public TransparentRNG(IEntropySource entropySource) : this()
        {
            _entropySource = entropySource;
        }

        public string GetAlgorithmName() => "HMAC-DRBG (HMAC-SHA256)";

        // ===== Initialize =====
        public void Initialize(byte[] seed)
        {
            lock (_sync)
            {
                if (seed == null || seed.Length == 0)
                    throw new ArgumentException("Seed cannot be empty");

                // Инициализация по стандарту
                _K = Enumerable.Repeat((byte)0x00, 32).ToArray();
                _V = Enumerable.Repeat((byte)0x01, 32).ToArray();

                Update(seed);
                OnGenerationStage?.Invoke(new GenerationStage(
                    "Initialized",
                    "DRBG инициализирован новым зерном",
                    seed,
                    DateTime.UtcNow
                ));
            }
        }

        public int Next() => Next(0, int.MaxValue);

        public int Next(int minValue, int maxValue)
        {
            if (minValue >= maxValue)
                throw new ArgumentException("minValue must be less than maxValue");

            lock (_sync)
            {
                byte[] block = GenerateBytes(4);
                int val = BitConverter.ToInt32(block, 0) & int.MaxValue;
                long range = (long)maxValue - minValue;
                int result = (int)(val % range) + minValue;

                OnGenerationStage?.Invoke(new GenerationStage(
                    "Generated",
                    $"Сгенерировано число: {result}",
                    block,
                    DateTime.UtcNow
                ));

                return result;
            }
        }

        public byte[] GetState()
        {
            lock (_sync)
            {
                // если _K или _V вдруг не инициализированы
                if (_K == null || _V == null)
                    return Array.Empty<byte>();

                return Concat(_K, _V);
            }
        }


        // ===== HMAC-DRBG internals =====
        private byte[] GenerateBytes(int count)
        {
            var output = new byte[count];
            int generated = 0;
            while (generated < count)
            {
                _V = Hmac(_V, _K);
                int toCopy = Math.Min(_V.Length, count - generated);
                Array.Copy(_V, 0, output, generated, toCopy);
                generated += toCopy;
            }

            //  Optional reseed step for extra diffusion
            Update(null);
            return output;
        }

        private void Update(byte[]? seed)
        {
            // K = HMAC(K, V || 0x00 || seed)
            byte[] temp = seed != null
                ? Concat(_V, new byte[] { 0x00 }, seed)
                : Concat(_V, new byte[] { 0x00 });
            _K = Hmac(temp, _K);
            _V = Hmac(_V, _K);

            // Если есть дополнительные данные, второй раунд
            if (seed != null)
            {
                temp = Concat(_V, new byte[] { 0x01 }, seed);
                _K = Hmac(temp, _K);
                _V = Hmac(_V, _K);
            }
        }

        private static byte[] Hmac(byte[] data, byte[] key)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(data);
        }

        private static byte[] Concat(params byte[][] parts)
        {
            int len = parts.Sum(p => p?.Length ?? 0);
            var r = new byte[len];
            int pos = 0;
            foreach (var p in parts)
            {
                if (p == null) continue;
                Array.Copy(p, 0, r, pos, p.Length);
                pos += p.Length;
            }
            return r;
        }

        public void Dispose()
        {
            Array.Clear(_K, 0, _K.Length);
            Array.Clear(_V, 0, _V.Length);
        }
    }
}
