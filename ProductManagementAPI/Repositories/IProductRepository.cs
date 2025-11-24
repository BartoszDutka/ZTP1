using ZtpLab1.Models;

namespace ZtpLab1.Repositories
{
    /// <summary>
    /// Specjalistyczny interfejs repozytorium dla produktów.
    /// Rozszerza podstawowy interfejs o operacje specyficzne dla produktów.
    /// </summary>
    public interface IProductRepository : IRepository<Product>
    {
        /// <summary>
        /// Pobiera podstawowe informacje o wszystkich produktach (tylko Id i Name).
        /// </summary>
        /// <returns>Lista obiektów z Id i Name</returns>
        Task<IEnumerable<object>> GetAllBasicInfoAsync();

        /// <summary>
        /// Sprawdza czy produkt o podanej nazwie już istnieje.
        /// </summary>
        /// <param name="name">Nazwa produktu</param>
        /// <param name="excludeId">ID produktu do wykluczenia z sprawdzenia (dla aktualizacji)</param>
        /// <returns>True jeśli nazwa już istnieje, false w przeciwnym razie</returns>
        Task<bool> IsNameExistsAsync(string name, string? excludeId = null);

        /// <summary>
        /// Pobiera produkty według kategorii.
        /// </summary>
        /// <param name="category">Nazwa kategorii</param>
        /// <returns>Lista produktów w danej kategorii</returns>
        Task<IEnumerable<Product>> GetByCategoryAsync(string category);
    }
}