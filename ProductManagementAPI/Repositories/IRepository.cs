namespace ZtpLab1.Repositories
{
    /// <summary>
    /// Generyczny interfejs repozytorium implementujący wzorzec Repository Pattern.
    /// Dostarcza podstawowe operacje CRUD z obsługą async/await.
    /// </summary>
    /// <typeparam name="T">Typ encji</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Pobiera wszystkie encje.
        /// </summary>
        /// <returns>Lista wszystkich encji</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Pobiera encję po identyfikatorze.
        /// </summary>
        /// <param name="id">Identyfikator encji</param>
        /// <returns>Encja lub null jeśli nie znaleziono</returns>
        Task<T?> GetByIdAsync(string id);

        /// <summary>
        /// Dodaje nową encję.
        /// </summary>
        /// <param name="entity">Encja do dodania</param>
        /// <returns>Dodana encja z wygenerowanym ID</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Aktualizuje istniejącą encję.
        /// </summary>
        /// <param name="id">Identyfikator encji</param>
        /// <param name="entity">Zaktualizowana encja</param>
        /// <returns>True jeśli aktualizacja powiodła się, false w przeciwnym razie</returns>
        Task<bool> UpdateAsync(string id, T entity);

        /// <summary>
        /// Usuwa encję po identyfikatorze.
        /// </summary>
        /// <param name="id">Identyfikator encji do usunięcia</param>
        /// <returns>True jeśli usunięcie powiodło się, false w przeciwnym razie</returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Sprawdza czy encja o podanym identyfikatorze istnieje.
        /// </summary>
        /// <param name="id">Identyfikator encji</param>
        /// <returns>True jeśli encja istnieje, false w przeciwnym razie</returns>
        Task<bool> ExistsAsync(string id);
    }
}