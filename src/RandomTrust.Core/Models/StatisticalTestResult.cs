namespace RandomTrust.Core.Models;

public class StatisticalTestResult
{
    public string TestName { get; set; } = string.Empty;
    public double PValue { get; set; }
    public bool Passed { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class FullTestResult
{
    public List<StatisticalTestResult> TestResults { get; set; } = new();
    public bool OverallResult { get; set; }
    public DateTime Timestamp { get; set; }
}
