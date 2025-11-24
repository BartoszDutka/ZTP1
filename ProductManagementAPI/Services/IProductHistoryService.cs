using ZtpLab1.Models;
using ZtpLab1.DataTransferObjects;

namespace ZtpLab1.Services
{
    /// <summary>
    /// Interfejs serwisu historii produktów.
    /// Definiuje operacje biznesowe związane z historią zmian produktów.
    /// </summary>
    public interface IProductHistoryService
    {
        /// <summary>
        /// Pobiera całą historię zmian wszystkich produktów.
        /// </summary>
        /// <returns>Lista wszystkich wpisów historii w odpowiedzi API</returns>
        Task<ApiResponse<IEnumerable<ProductHistory>>> GetAllHistoryAsync();

        /// <summary>
        /// Pobiera historię zmian dla określonego produktu.
        /// </summary>
        /// <param name="productId">Identyfikator produktu</param>
        /// <returns>Lista wpisów historii w odpowiedzi API</returns>
        Task<ApiResponse<IEnumerable<ProductHistory>>> GetProductHistoryAsync(string productId);

        /// <summary>
        /// Zapisuje wpis historii dla operacji CREATE.
        /// </summary>
        /// <param name="product">Utworzony produkt</param>
        /// <returns>Task</returns>
        Task RecordCreateHistoryAsync(Product product);

        /// <summary>
        /// Zapisuje wpis historii dla operacji UPDATE.
        /// </summary>
        /// <param name="previousProduct">Poprzedni stan produktu</param>
        /// <param name="currentProduct">Aktualny stan produktu</param>
        /// <returns>Task</returns>
        Task RecordUpdateHistoryAsync(Product previousProduct, Product currentProduct);

        /// <summary>
        /// Zapisuje wpis historii dla operacji DELETE.
        /// </summary>
        /// <param name="product">Usunięty produkt</param>
        /// <returns>Task</returns>
        Task RecordDeleteHistoryAsync(Product product);
    }
}