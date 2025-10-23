using RandomTrust.Core.EntropySources;
using RandomTrust.Core.Generators;
using RandomTrust.Core.Interfaces;
using Xunit;

namespace RandomTrust.Tests.UnitTests;

public class BasicTests
{
    [Fact]
    public void EntropySource_CanGenerateSeed()
    {
        // Arrange
        var source = new HybridEntropySource();

        // Act — добавляем достаточно энтропийных событий
        for (int i = 0; i < 150; i++)
        {
            source.AddUserEntropy(i * 0.01, Math.Sin(i * 0.1), i % 7, DateTime.UtcNow.Ticks);
        }

        // Проверяем, что энтропия действительно накопилась ДО генерации
        Assert.True(source.CurrentEntropyLevel >= 100.0, $"Entropy level must reach 100%, got {source.CurrentEntropyLevel:F2}%");

        var seed = source.GenerateSeed();

        // Assert
        Assert.NotNull(seed);
        Assert.True(seed.Length == 32 || seed.Length == 64, $"Seed length must be 32 or 64 bytes, got {seed.Length}");
    }




    [Fact]
    public void Generator_CanBeInitialized()
    {
        // Arrange
        var source = new HybridEntropySource();

        for (int i = 0; i < 120; i++)
        {
            source.AddUserEntropy(i * 0.5, Math.Cos(i), i % 3);
        }

        var seed = source.GenerateSeed();
        var generator = new TransparentRNG(source);

        // Act
        generator.Initialize(seed);

        // Assert
        var state = generator.GetState();
        Assert.NotNull(state);
        Assert.True(state.Length > 0, "State must contain data after initialization");
    }

    [Fact]
    public void Generator_ProducesDifferentNumbers()
    {
        // Arrange
        var source = new HybridEntropySource();

        for (int i = 0; i < 110; i++)
        {
            source.AddUserEntropy(i * 0.3, Math.Tan(i * 0.01), i % 5);
        }

        var seed = source.GenerateSeed();
        var generator = new TransparentRNG(source);
        generator.Initialize(seed);

        // Act
        var numbers = new HashSet<int>();
        for (int i = 0; i < 100; i++)
        {
            numbers.Add(generator.Next(1, 1000));
        }

        // Assert
        Assert.True(numbers.Count > 50, $"Expected more random diversity, got only {numbers.Count} unique numbers");
    }

    [Fact]
    public void StatisticalTester_Works()
    {
        // Arrange
        var tester = new StatisticalTester();
        var testData = new byte[1000];
        new Random().NextBytes(testData);

        // Act
        var result = tester.TestFrequencyMonobit(testData);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.TestName));
        Assert.InRange(result.PValue, 0.0, 1.0);
    }

    [Fact]
    public void EntropySource_RequiresMinimumData()
    {
        // Arrange
        var source = new HybridEntropySource();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => source.GenerateSeed());

        for (int i = 0; i < 3; i++)
        {
            source.AddUserEntropy(i);
        }

        Assert.Throws<InvalidOperationException>(() => source.GenerateSeed());
    }
}
