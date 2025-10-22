using RandomTrust.Core.Models;

namespace RandomTrust.Core.Interfaces;

/// <summary>
/// Генератор случайных чисел с возможностью верификации
/// </summary>
public interface IRandomGenerator
{
    void Initialize(byte[] seed);
    int Next();
    int Next(int minValue, int maxValue);
    byte[] GetState();
    string GetAlgorithmName();
    
    event Action<GenerationStage> OnGenerationStage;
}
