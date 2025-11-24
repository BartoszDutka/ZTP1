using ZtpLab1.DataTransferObjects;
using ZtpLab1.Models;

namespace ZtpLab1.Services
{
    /// <summary>
    /// Interfejs serwisu do zarządzania produktami.
    /// Zawiera całą logikę biznesową związaną z produktami.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Pobiera podstawowe informacje o wszystkich produktach.
        /// </summary>
        /// <returns>Response z listą podstawowych informacji o produktach</returns>
        Task<ApiResponse<IEnumerable<object>>> GetAllBasicAsync();

        /// <summary>
        /// Pobiera szczegółowe informacje o produkcie.
        /// </summary>
        /// <param name="id">Identyfikator produktu</param>
        /// <returns>Response ze szczegółami produktu</returns>
        Task<ApiResponse<Product>> GetByIdAsync(string id);

        /// <summary>
        /// Tworzy nowy produkt z pełną walidacją.
        /// </summary>
        /// <param name="createDto">Dane nowego produktu</param>
        /// <returns>Response z utworzonym produktem</returns>
        Task<ApiResponse<Product>> CreateAsync(CreateProductDto createDto);

        /// <summary>
        /// Aktualizuje istniejący produkt z pełną walidacją.
        /// </summary>
        /// <param name="id">Identyfikator produktu</param>
        /// <param name="updateDto">Nowe dane produktu</param>
        /// <returns>Response z wynikiem operacji</returns>
        Task<ApiResponse<Product>> UpdateAsync(string id, UpdateProductDto updateDto);

        /// <summary>
        /// Usuwa produkt.
        /// </summary>
        /// <param name="id">Identyfikator produktu</param>
        /// <returns>Response z wynikiem operacji</returns>
        Task<ApiResponse> DeleteAsync(string id);
    }
}