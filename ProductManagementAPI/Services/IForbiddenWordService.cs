using ZtpLab1.DataTransferObjects;
using ZtpLab1.Models;

namespace ZtpLab1.Services
{
    /// <summary>
    /// Interfejs serwisu do zarządzania zabronionymi słowami.
    /// Zawiera całą logikę biznesową związaną z zabronionymi słowami.
    /// </summary>
    public interface IForbiddenWordService
    {
        /// <summary>
        /// Pobiera wszystkie zabronione słowa.
        /// </summary>
        /// <returns>Response z listą zabronionych słów</returns>
        Task<ApiResponse<IEnumerable<ForbiddenWord>>> GetAllAsync();

        /// <summary>
        /// Pobiera tylko aktywne zabronione słowa.
        /// </summary>
        /// <returns>Response z listą aktywnych zabronionych słów</returns>
        Task<ApiResponse<IEnumerable<ForbiddenWord>>> GetActiveAsync();

        /// <summary>
        /// Pobiera zabronione słowo po identyfikatorze.
        /// </summary>
        /// <param name="id">Identyfikator słowa</param>
        /// <returns>Response z zabronionym słowem</returns>
        Task<ApiResponse<ForbiddenWord>> GetByIdAsync(string id);

        /// <summary>
        /// Tworzy nowe zabronione słowo z walidacją.
        /// </summary>
        /// <param name="createDto">Dane nowego słowa</param>
        /// <returns>Response z utworzonym słowem</returns>
        Task<ApiResponse<ForbiddenWord>> CreateAsync(CreateForbiddenWordDto createDto);

        /// <summary>
        /// Aktualizuje istniejące zabronione słowo z walidacją.
        /// </summary>
        /// <param name="id">Identyfikator słowa</param>
        /// <param name="updateDto">Nowe dane słowa</param>
        /// <returns>Response z wynikiem operacji</returns>
        Task<ApiResponse<ForbiddenWord>> UpdateAsync(string id, UpdateForbiddenWordDto updateDto);

        /// <summary>
        /// Usuwa zabronione słowo.
        /// </summary>
        /// <param name="id">Identyfikator słowa</param>
        /// <returns>Response z wynikiem operacji</returns>
        Task<ApiResponse> DeleteAsync(string id);

        /// <summary>
        /// Przełącza status aktywności słowa.
        /// </summary>
        /// <param name="id">Identyfikator słowa</param>
        /// <returns>Response ze zaktualizowanym słowem</returns>
        Task<ApiResponse<ForbiddenWord>> ToggleActiveStatusAsync(string id);

        /// <summary>
        /// Pobiera aktywne zabronione słowa do walidacji.
        /// </summary>
        /// <returns>Lista aktywnych zabronionych słów</returns>
        Task<IEnumerable<ForbiddenWord>> GetActiveForbiddenWordsForValidationAsync();
    }
}