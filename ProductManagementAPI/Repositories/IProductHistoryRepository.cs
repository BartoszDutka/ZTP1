using ZtpLab1.Models;

namespace ZtpLab1.Repositories
{
    /// <summary>
    /// Interfejs repozytorium historii produktów.
    /// Definiuje operacje dostępu do danych historii zmian produktów.
    /// </summary>
    public interface IProductHistoryRepository : IRepository<ProductHistory>
    {
        /// <summary>
        /// Pobiera historię zmian dla określonego produktu.
        /// </summary>
        /// <param name="productId">Identyfikator produktu</param>
        /// <returns>Lista wpisów historii uporządkowana chronologicznie (najnowsze pierwsze)</returns>
        Task<IEnumerable<ProductHistory>> GetHistoryByProductIdAsync(string productId);

        /// <summary>
        /// Dodaje wpis historii dla produktu.
        /// </summary>
        /// <param name="history">Wpis historii do dodania</param>
        /// <returns>Dodany wpis historii</returns>
        Task<ProductHistory> AddHistoryAsync(ProductHistory history);

        /// <summary>
        /// Pobiera najnowszy wpis historii dla produktu.
        /// </summary>
        /// <param name="productId">Identyfikator produktu</param>
        /// <returns>Najnowszy wpis historii lub null jeśli nie istnieje</returns>
        Task<ProductHistory?> GetLatestHistoryByProductIdAsync(string productId);
    }
}