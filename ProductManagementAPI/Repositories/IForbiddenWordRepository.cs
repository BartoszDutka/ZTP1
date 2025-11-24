using ZtpLab1.Models;

namespace ZtpLab1.Repositories
{
    /// <summary>
    /// Specjalistyczny interfejs repozytorium dla zabronionych słów.
    /// Rozszerza podstawowy interfejs o operacje specyficzne dla zabronionych słów.
    /// </summary>
    public interface IForbiddenWordRepository : IRepository<ForbiddenWord>
    {
        /// <summary>
        /// Pobiera wszystkie aktywne zabronione słowa.
        /// </summary>
        /// <returns>Lista aktywnych zabronionych słów</returns>
        Task<IEnumerable<ForbiddenWord>> GetActiveAsync();

        /// <summary>
        /// Sprawdza czy słowo o podanej treści już istnieje.
        /// </summary>
        /// <param name="word">Treść słowa</param>
        /// <param name="excludeId">ID słowa do wykluczenia z sprawdzenia (dla aktualizacji)</param>
        /// <returns>True jeśli słowo już istnieje, false w przeciwnym razie</returns>
        Task<bool> IsWordExistsAsync(string word, string? excludeId = null);

        /// <summary>
        /// Przełącza status aktywności słowa.
        /// </summary>
        /// <param name="id">ID słowa</param>
        /// <returns>Zaktualizowane słowo lub null jeśli nie znaleziono</returns>
        Task<ForbiddenWord?> ToggleActiveStatusAsync(string id);
    }
}