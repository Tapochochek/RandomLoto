using RandomTrust.Core.Interfaces;
using RandomTrust.Core.Models;

namespace RandomTrust.Services;

public class VerificationService
{
    private readonly IStatisticalTester _tester;
    
    public VerificationService(IStatisticalTester tester)
    {
        _tester = tester;
    }
    
    public VerificationResult VerifyDraw(LotteryDraw draw)
    {
        var testResult = _tester.RunAllTests(draw.GeneratorState);
        
        return new VerificationResult
        {
            DrawId = draw.DrawId,
            TestResults = testResult,
            IsValid = testResult.OverallResult,
            VerificationTime = DateTime.UtcNow
        };
    }
    
    public async Task<VerificationResult> VerifyHistoricalDraw(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Файл с данными тиража не найден");
        }
        
        var json = await File.ReadAllTextAsync(filePath);
        var draw = System.Text.Json.JsonSerializer.Deserialize<LotteryDraw>(json);
        
        if (draw == null)
        {
            throw new InvalidOperationException("Не удалось прочитать данные тиража");
        }
        
        return VerifyDraw(draw);
    }
}

public class VerificationResult
{
    public string DrawId { get; set; } = string.Empty;
    public FullTestResult TestResults { get; set; } = new();
    public bool IsValid { get; set; }
    public DateTime VerificationTime { get; set; }
}
