using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

namespace RandomTrust.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestRunnerController : ControllerBase
    {
        [HttpPost("analyze")]
        public IActionResult Analyze([FromBody] NumberAnalysisRequest request)
        {
            if (request?.Numbers == null || request.Numbers.Count == 0)
                return BadRequest("Список чисел пуст");

            var numbers = request.Numbers.Select(n => (byte)(n % 256)).ToArray();
            var tester = new StatisticalTester();
            var tests = new List<object>();

            // 🔹 Запускаем все тесты (8 штук)
            tests.Add(RunTest("Frequency Monobit Test", tester.TestFrequencyMonobit, numbers));
            tests.Add(RunTest("Runs Test", tester.TestRuns, numbers));
            tests.Add(RunTest("Serial Test", tester.TestSerial, numbers));
            tests.Add(RunTest("Poker Test", tester.TestPoker, numbers));
            tests.Add(RunTest("Chi-Square Test", tester.TestChiSquare, numbers));
            tests.Add(RunTest("Autocorrelation Test", tester.TestAutocorrelation, numbers));
            tests.Add(RunTest("Cumulative Sums Test", tester.TestCumulativeSums, numbers));
            tests.Add(RunTest("Approximate Entropy Test", tester.TestApproximateEntropy, numbers));

            // 🔹 Общая статистика последовательности
            var mean = request.Numbers.Average();
            var stdDev = Math.Sqrt(request.Numbers.Average(v => Math.Pow(v - mean, 2)));

            return Ok(new
            {
                count = request.Numbers.Count,
                mean,
                min = request.Numbers.Min(),
                max = request.Numbers.Max(),
                stdDev,
                sha256 = ComputeSha256(request.Numbers),
                tests
            });
        }

        // 🔹 Безопасный запуск теста с try/catch
        private object RunTest(string name, Func<byte[], StatisticalTestResult> testFunc, byte[] data)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var result = testFunc(data);
                sw.Stop();

                return new
                {
                    testName = name,
                    pValue = result.PValue,
                    passed = result.Passed,
                    durationMs = Math.Round(sw.Elapsed.TotalMilliseconds, 3)
                };
            }
            catch (Exception ex)
            {
                sw.Stop();
                return new
                {
                    testName = name,
                    pValue = double.NaN,
                    passed = false,
                    durationMs = Math.Round(sw.Elapsed.TotalMilliseconds, 3),
                    error = ex.Message
                };
            }
        }

        // 🔹 Хэш SHA-256 для последовательности
        private string ComputeSha256(IEnumerable<int> numbers)
        {
            using var sha = SHA256.Create();
            var bytes = numbers.Select(n => (byte)(n % 256)).ToArray();
            var hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    // 🔹 Класс запроса
    public class NumberAnalysisRequest
    {
        public List<int> Numbers { get; set; } = new();
    }

    // 🔹 Реализация статистических тестов
    public class StatisticalTester
    {
        private static readonly Random _rnd = new();

        public StatisticalTestResult TestFrequencyMonobit(byte[] data)
        {
            int ones = data.Sum(b => Convert.ToString(b, 2).Count(c => c == '1'));
            int zeros = data.Length * 8 - ones;
            double s = Math.Abs(ones - zeros) / Math.Sqrt(data.Length * 8);
            double pValue = Math.Exp(-2 * Math.Pow(s, 2) / data.Length);
            return new StatisticalTestResult(pValue >= 0.01, pValue);
        }

        public StatisticalTestResult TestRuns(byte[] data)
        {
            if (data.Length < 2)
                return new StatisticalTestResult(false, 0);

            int runs = 1;
            for (int i = 1; i < data.Length; i++)
                if (data[i] != data[i - 1]) runs++;

            double expectedRuns = (2 * runs - 1) / (double)data.Length;
            double pValue = Math.Exp(-Math.Abs(expectedRuns - 1));
            return new StatisticalTestResult(pValue >= 0.01, pValue);
        }

        public StatisticalTestResult TestSerial(byte[] data)
        {
            if (data.Length < 2)
                return new StatisticalTestResult(false, 0);

            double mean = data.Average(b => (double)b);
            double variance = data.Average(b => Math.Pow(b - mean, 2));
            double serialCorr = 1 - Math.Abs(variance / 10000.0);
            return new StatisticalTestResult(serialCorr > 0.05, serialCorr);
        }

        public StatisticalTestResult TestPoker(byte[] data)
        {
            int k = 4;
            int m = data.Length / k;
            if (m == 0) return new StatisticalTestResult(false, 0);

            var counts = new Dictionary<string, int>();
            for (int i = 0; i < m; i++)
            {
                var block = string.Join("", data.Skip(i * k).Take(k));
                if (!counts.ContainsKey(block)) counts[block] = 0;
                counts[block]++;
            }

            double chi2 = counts.Values.Sum(v => Math.Pow(v - (m / (double)counts.Count), 2)) / (m / (double)counts.Count);
            double pValue = Math.Exp(-chi2 / 2);
            return new StatisticalTestResult(pValue >= 0.01, pValue);
        }

        public StatisticalTestResult TestChiSquare(byte[] data)
        {
            if (data.Length == 0)
                return new StatisticalTestResult(false, 0);

            double expected = data.Length / 256.0;
            double chi2 = Enumerable.Range(0, 256).Sum(i =>
            {
                double obs = data.Count(b => b == i);
                return Math.Pow(obs - expected, 2) / expected;
            });

            double pValue = Math.Exp(-chi2 / (2 * 256));
            return new StatisticalTestResult(pValue >= 0.01, pValue);
        }

        public StatisticalTestResult TestAutocorrelation(byte[] data)
        {
            if (data.Length < 2)
                return new StatisticalTestResult(false, 0);

            double mean = data.Average(b => (double)b);
            double num = 0, den = 0;
            for (int i = 1; i < data.Length; i++)
            {
                num += (data[i] - mean) * (data[i - 1] - mean);
                den += Math.Pow(data[i] - mean, 2);
            }
            double autocorr = num / den;
            double pValue = 1 - Math.Abs(autocorr);
            return new StatisticalTestResult(pValue >= 0.01, pValue);
        }

        public StatisticalTestResult TestCumulativeSums(byte[] data)
        {
            int sum = 0, max = 0;
            foreach (var b in data)
            {
                sum += (b % 2 == 0) ? 1 : -1;
                max = Math.Max(max, Math.Abs(sum));
            }
            double pValue = Math.Exp(-2.0 * Math.Pow(max / (double)data.Length, 2));
            return new StatisticalTestResult(pValue >= 0.01, pValue);
        }

        public StatisticalTestResult TestApproximateEntropy(byte[] data)
        {
            if (data.Length == 0)
                return new StatisticalTestResult(false, 0);

            double entropy = -data.GroupBy(b => b)
                .Select(g => (double)g.Count() / data.Length)
                .Sum(p => p * Math.Log(p, 2));

            double normalized = entropy / 8.0; // нормировка до [0,1]
            return new StatisticalTestResult(normalized >= 0.3, normalized);
        }
    }

    // 🔹 Результат теста
    public class StatisticalTestResult
    {
        public bool Passed { get; set; }
        public double PValue { get; set; }

        public StatisticalTestResult(bool passed, double pValue)
        {
            Passed = passed;
            PValue = pValue;
        }
    }
}
