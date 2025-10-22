using RandomTrust.Core.Models;

namespace RandomTrust.Core.Interfaces;

public interface IStatisticalTester
{
    StatisticalTestResult TestFrequencyMonobit(byte[] data);
    StatisticalTestResult TestRunsTest(byte[] data);
    FullTestResult RunAllTests(byte[] data);
}
