namespace RandomTrust.Core.Interfaces;

/// <summary>
/// Источник энтропии для генератора случайных чисел
/// </summary>
public interface IEntropySource
{
    string Name { get; }
    double CurrentEntropyLevel { get; }
    void AddUserEntropy(params double[] data);
    byte[] GenerateSeed();
    void Reset();
}
