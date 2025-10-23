namespace RandomTrust.Core.Models;

/// <summary>
/// Этап генерации случайного числа для визуализации
/// </summary>
public record GenerationStage(
    string StageName, 
    string Description, 
    byte[] Data, 
    DateTime Timestamp
);
