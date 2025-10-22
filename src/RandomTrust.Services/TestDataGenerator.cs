using RandomTrust.Core.Interfaces;

namespace RandomTrust.Services;

public class TestDataGenerator
{
    private readonly IRandomGenerator _rng;
    
    public TestDataGenerator(IRandomGenerator rng)
    {
        _rng = rng;
    }
    
    public async Task<string> GenerateNISTTestFileAsync(string directoryPath = "test_data", 
        int bitCount = 1_000_000)
    {
        Directory.CreateDirectory(directoryPath);
        
        var fileName = $"nist_test_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        var filePath = Path.Combine(directoryPath, fileName);
        
        Console.WriteLine($"Starting generation of {bitCount} bits...");
        
        await using var writer = new StreamWriter(filePath);
        
        int generatedBits = 0;
        int batchCount = 0;
        
        while (generatedBits < bitCount)
        {
            // Генерируем 32 бита за раз
            var randomValue = _rng.Next();
            for (int i = 0; i < 32 && generatedBits < bitCount; i++)
            {
                var bit = (randomValue & (1 << i)) != 0;
                await writer.WriteAsync(bit ? '1' : '0');
                generatedBits++;
            }
            
            // Прогресс каждые 100K бит
            if (generatedBits % 100000 == 0)
            {
                batchCount++;
                Console.WriteLine($"Generated {generatedBits} bits ({batchCount * 10}%)");
                await writer.FlushAsync();
            }
            
            // Не блокировать поток
            if (generatedBits % 10000 == 0)
            {
                await Task.Yield();
            }
        }
        
        Console.WriteLine($"✅ Successfully generated {generatedBits} bits for NIST testing: {filePath}");
        Console.WriteLine($"📁 File size: {new FileInfo(filePath).Length} bytes");
        
        return filePath;
    }
    
    public async Task<string> GenerateLotterySequenceAsync(int numbersCount, int minValue, int maxValue)
    {
        var numbers = new List<int>();
        for (int i = 0; i < numbersCount; i++)
        {
            numbers.Add(_rng.Next(minValue, maxValue + 1));
        }
        
        var fileName = $"lottery_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        var filePath = Path.Combine("test_data", fileName);
        
        var result = new {
            Numbers = numbers,
            Timestamp = DateTime.UtcNow,
            GeneratorState = _rng.GetState()
        };
        
        await File.WriteAllTextAsync(filePath, 
            System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            }));
            
        Console.WriteLine($"Generated lottery numbers: {string.Join(", ", numbers)}");
        return filePath;
    }
}
