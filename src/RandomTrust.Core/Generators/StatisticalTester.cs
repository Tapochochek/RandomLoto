using RandomTrust.Core.Interfaces;
using RandomTrust.Core.Models;
using RandomTrust.Core.Utilities;

namespace RandomTrust.Core.Generators;

public class StatisticalTester : IStatisticalTester
{
    public StatisticalTestResult TestFrequencyMonobit(byte[] data)
    {
        int ones = 0;
        foreach (byte b in data)
        {
            ones += CountOnes(b);
        }
        
        int n = data.Length * 8;
        double s = Math.Abs(ones - (n - ones)) / Math.Sqrt(n);
        double pValue = MathHelper.Erfc(s / Math.Sqrt(2));
        
        return new StatisticalTestResult
        {
            TestName = "Frequency Monobit Test",
            PValue = pValue,
            Passed = pValue > 0.01,
            Description = $"Доля единиц: {(double)ones/n:P2}, p-value: {pValue:F6}"
        };
    }
    
    public StatisticalTestResult TestRunsTest(byte[] data)
    {
        var bits = GetBits(data);
        if (bits.Length < 100) 
        {
            return new StatisticalTestResult
            {
                TestName = "Runs Test",
                PValue = 0,
                Passed = false,
                Description = "Недостаточно данных для теста (нужно минимум 100 бит)"
            };
        }
        
        int runs = 1;
        bool currentBit = bits[0];
        
        for (int i = 1; i < bits.Length; i++)
        {
            if (bits[i] != currentBit)
            {
                runs++;
                currentBit = bits[i];
            }
        }
        
        int onesCount = bits.Count(b => b);
        double pi = (double)onesCount / bits.Length;
        double expectedRuns = 2 * bits.Length * pi * (1 - pi);
        double variance = expectedRuns * (1 - 2 * pi * (1 - pi)) / (bits.Length - 1);
        
        if (variance <= 0) 
        {
            return new StatisticalTestResult
            {
                TestName = "Runs Test",
                PValue = 0,
                Passed = false,
                Description = "Ошибка в расчете дисперсии"
            };
        }
        
        double z = (runs - expectedRuns) / Math.Sqrt(variance);
        double pValue = 2 * (1 - MathHelper.NormalCdf(Math.Abs(z)));
        
        return new StatisticalTestResult
        {
            TestName = "Runs Test",
            PValue = pValue,
            Passed = pValue > 0.01,
            Description = $"Серии: {runs}, Ожидаемые: {expectedRuns:F2}, p-value: {pValue:F6}"
        };
    }
    
    public FullTestResult RunAllTests(byte[] data)
    {
        var results = new List<StatisticalTestResult>
        {
            TestFrequencyMonobit(data),
            TestRunsTest(data)
        };
        
        return new FullTestResult
        {
            TestResults = results,
            OverallResult = results.All(r => r.Passed),
            Timestamp = DateTime.UtcNow
        };
    }
    
    private int CountOnes(byte b)
    {
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            if ((b & (1 << i)) != 0) count++;
        }
        return count;
    }
    
    private bool[] GetBits(byte[] data)
    {
        var bits = new bool[data.Length * 8];
        for (int i = 0; i < data.Length; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                bits[i * 8 + j] = (data[i] & (1 << j)) != 0;
            }
        }
        return bits;
    }
}
