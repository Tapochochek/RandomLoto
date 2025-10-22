using System;

namespace RandomTrust.Core.Interfaces
{
    /// <summary>
    /// Интерфейс для источников энтропии.
    /// </summary>
    public interface IEntropyGenerator
    {
        /// <summary>
        /// Добавляет пользовательскую энтропию.
        /// </summary>
        void AddUserEntropy(params object[] values);

        /// <summary>
        /// Генерирует 32-байтное (256-битное) зерно (seed) на основе накопленной энтропии.
        /// </summary>
        byte[] GenerateSeed();

        /// <summary>
        /// Возвращает текущий уровень энтропии (0..1).
        /// </summary>
        double CurrentEntropyLevel { get; }

        /// <summary>
        /// Событие при обновлении энтропии.
        /// </summary>
        event Action<double>? OnEntropyUpdated;
    }
}
