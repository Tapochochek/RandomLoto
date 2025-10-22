using Microsoft.AspNetCore.Mvc;
using RandomTrust.Core.Interfaces;
using RandomTrust.Core.Models;
using RandomTrust.Services;

namespace RandomTrust.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RandomController : ControllerBase
{
    private readonly IRandomGenerator _rng;
    private readonly IEntropySource _entropySource;
    private readonly TestDataGenerator _testGenerator;
    private readonly IStatisticalTester _tester;
    
    public RandomController(IRandomGenerator rng, IEntropySource entropySource, 
                          TestDataGenerator testGenerator, IStatisticalTester tester)
    {
        _rng = rng;
        _entropySource = entropySource;
        _testGenerator = testGenerator;
        _tester = tester;
    }
    
    [HttpPost("entropy")]
    public IActionResult AddEntropy([FromBody] MouseData data)
    {
        _entropySource.AddUserEntropy(data.X, data.Y, data.Velocity, data.Timestamp);
        return Ok(new { 
            entropyLevel = _entropySource.CurrentEntropyLevel,
            message = $"Энтропия добавлена. Уровень: {_entropySource.CurrentEntropyLevel:P2}"
        });
    }
    
    [HttpPost("initialize")]
    public IActionResult InitializeGenerator()
    {
        try
        {
            var seed = _entropySource.GenerateSeed();
            _rng.Initialize(seed);
            return Ok(new { 
                message = "Генератор инициализирован", 
                seed = Convert.ToBase64String(seed),
                algorithm = _rng.GetAlgorithmName()
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    [HttpGet("lottery/{count}")]
    public IActionResult GenerateLotteryNumbers(int count, [FromQuery] int min = 1, [FromQuery] int max = 45)
    {
        try
        {
            var numbers = new List<int>();
            for (int i = 0; i < count; i++)
            {
                numbers.Add(_rng.Next(min, max + 1));
            }
            
            var draw = LotteryDraw.Create(numbers.ToArray(), _rng.GetState(), _rng.GetState());
            
            return Ok(new {
                numbers,
                timestamp = DateTime.UtcNow,
                drawId = draw.DrawId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    [HttpPost("generate-test-data")]
    public async Task<IActionResult> GenerateTestData([FromQuery] int bitCount = 1000000)
    {
        try
        {
            var filePath = await _testGenerator.GenerateNISTTestFileAsync(bitCount: bitCount);
            return Ok(new { 
                message = "Тестовые данные сгенерированы",
                filePath,
                fileSize = new FileInfo(filePath).Length,
                bitsGenerated = bitCount
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    [HttpPost("run-statistical-tests")]
    public IActionResult RunStatisticalTests([FromQuery] int dataSize = 10000)
    {
        try
        {
            // Генерируем тестовые данные
            var testData = new byte[dataSize];
            for (int i = 0; i < dataSize; i++)
            {
                testData[i] = (byte)_rng.Next(0, 256);
            }
            
            // Запускаем все тесты
            var result = _tester.RunAllTests(testData);
            
            return Ok(new {
                message = "Статистические тесты завершены",
                overallResult = result.OverallResult ? "PASSED" : "FAILED",
                testsCount = result.TestResults.Count,
                testsPassed = result.TestResults.Count(r => r.Passed),
                timestamp = result.Timestamp,
                results = result.TestResults
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public record MouseData(double X, double Y, double Velocity, long Timestamp);
