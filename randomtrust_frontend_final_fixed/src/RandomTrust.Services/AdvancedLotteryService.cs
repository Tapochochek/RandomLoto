using RandomTrust.Core.Interfaces;
using RandomTrust.Core.Models;

namespace RandomTrust.Services;

public class AdvancedLotteryService
{
    private readonly IRandomGenerator _rng;
    
    public AdvancedLotteryService(IRandomGenerator rng)
    {
        _rng = rng;
    }
    
    public LotteryDraw GenerateUniqueNumbers(int count, int minValue, int maxValue)
    {
        if (count > (maxValue - minValue + 1))
            throw new ArgumentException("Нельзя сгенерировать больше уникальных чисел чем доступно в диапазоне");
            
        var numbers = new HashSet<int>();
        
        // Генерируем уникальные числа
        while (numbers.Count < count)
        {
            var number = _rng.Next(minValue, maxValue + 1);
            numbers.Add(number);
        }
        
        var uniqueNumbers = numbers.ToArray();
        
        return LotteryDraw.Create(uniqueNumbers, _rng.GetState(), _rng.GetState());
    }
    
    public async Task<string> GenerateNISTTestFileAdvanced(string directoryPath = "test_data", 
        int bitCount = 1_000_000)
    {
        Directory.CreateDirectory(directoryPath);
        
        var fileName = $"nist_advanced_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        var filePath = Path.Combine(directoryPath, fileName);
        
        await using var writer = new StreamWriter(filePath);
        
        int generatedBits = 0;
        while (generatedBits < bitCount)
        {
            var randomValue = _rng.Next();
            for (int i = 0; i < 32 && generatedBits < bitCount; i++)
            {
                var bit = (randomValue & (1 << i)) != 0;
                await writer.WriteAsync(bit ? '1' : '0');
                generatedBits++;
            }
            
            if (generatedBits % 10000 == 0)
            {
                await writer.FlushAsync();
                await Task.Yield();
            }
        }
        
        Console.WriteLine($"✅ Generated {generatedBits} bits for NIST: {filePath}");
        return filePath;
    }
}
