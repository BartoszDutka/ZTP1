using System.ComponentModel.DataAnnotations;
using ZtpLab1.DataTransferObjects;
using ZtpLab1.Models;
using ZtpLab1.Repositories;
using ZtpLab1.BusinessRules.ForbiddenWord;

namespace ZtpLab1.Services
{
    /// <summary>
    /// Implementacja serwisu zabronionych słów zawierająca logikę biznesową.
    /// Używa wzorca Specification Pattern do walidacji i Repository Pattern do dostępu do danych.
    /// </summary>
    public class ForbiddenWordService : IForbiddenWordService
    {
        private readonly IForbiddenWordRepository _forbiddenWordRepository;

        public ForbiddenWordService(IForbiddenWordRepository forbiddenWordRepository)
        {
            _forbiddenWordRepository = forbiddenWordRepository;
        }

        public async Task<ApiResponse<IEnumerable<ForbiddenWord>>> GetAllAsync()
        {
            try
            {
                var words = await _forbiddenWordRepository.GetAllAsync();
                return ApiResponse<IEnumerable<ForbiddenWord>>.SuccessResponse(words, "Pobrano listę zabronionych słów pomyślnie.");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<ForbiddenWord>>.ErrorResponse(
                    $"Błąd podczas pobierania listy zabronionych słów: {ex.Message}", 
                    500
                );
            }
        }

        public async Task<ApiResponse<IEnumerable<ForbiddenWord>>> GetActiveAsync()
        {
            try
            {
                var words = await _forbiddenWordRepository.GetActiveAsync();
                return ApiResponse<IEnumerable<ForbiddenWord>>.SuccessResponse(words, "Pobrano listę aktywnych zabronionych słów pomyślnie.");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<ForbiddenWord>>.ErrorResponse(
                    $"Błąd podczas pobierania aktywnych zabronionych słów: {ex.Message}", 
                    500
                );
            }
        }

        public async Task<ApiResponse<ForbiddenWord>> GetByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return ApiResponse<ForbiddenWord>.CreateError("Identyfikator słowa jest wymagany.", 400);
                }

                var word = await _forbiddenWordRepository.GetByIdAsync(id);
                if (word == null)
                {
                    return ApiResponse<ForbiddenWord>.CreateError($"Zabronione słowo o ID {id} nie zostało znalezione.", 404);
                }

                return ApiResponse<ForbiddenWord>.CreateSuccess(word, "Pobrano szczegóły zakazanego słowa pomyślnie.");
            }
            catch (Exception ex)
            {
                return ApiResponse<ForbiddenWord>.CreateError(
                    $"Błąd podczas pobierania zakazanego słowa: {ex.Message}", 
                    500
                );
            }
        }

        public async Task<ApiResponse<ForbiddenWord>> CreateAsync(CreateForbiddenWordDto createDto)
        {
            try
            {
                // Mapowanie DTO na model
                var forbiddenWord = new ForbiddenWord
                {
                    Word = createDto.Word?.Trim() ?? string.Empty,
                    Reason = createDto.Reason?.Trim(),
                    IsActive = createDto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                // Walidacja przy użyciu wzorca Specification
                var validationResult = ValidateForbiddenWord(forbiddenWord);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                // Sprawdzenie czy słowo już istnieje
                if (await _forbiddenWordRepository.IsWordExistsAsync(forbiddenWord.Word))
                {
                    return ApiResponse<ForbiddenWord>.CreateError(
                        $"Zabronione słowo '{forbiddenWord.Word}' już istnieje.", 
                        409 // Konflikt
                    );
                }

                // Zapis w repozytorium
                var createdWord = await _forbiddenWordRepository.AddAsync(forbiddenWord);
                return ApiResponse<ForbiddenWord>.CreateSuccess(createdWord, "Zabronione słowo zostało utworzone pomyślnie.");
            }
            catch (InvalidOperationException ex)
            {
                // Błąd unikalności z bazy danych
                return ApiResponse<ForbiddenWord>.CreateError(ex.Message, 409);
            }
            catch (Exception ex)
            {
                return ApiResponse<ForbiddenWord>.CreateError(
                    $"Błąd podczas tworzenia zakazanego słowa: {ex.Message}", 
                    500
                );
            }
        }

        public async Task<ApiResponse<ForbiddenWord>> UpdateAsync(string id, UpdateForbiddenWordDto updateDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return ApiResponse<ForbiddenWord>.CreateError("Identyfikator słowa jest wymagany.", 400);
                }

                // Sprawdzenie czy słowo istnieje
                var existingWord = await _forbiddenWordRepository.GetByIdAsync(id);
                if (existingWord == null)
                {
                    return ApiResponse<ForbiddenWord>.CreateError($"Zabronione słowo o ID {id} nie zostało znalezione.", 404);
                }

                // Mapowanie DTO na model (zachowanie ID i CreatedAt)
                var forbiddenWord = new ForbiddenWord
                {
                    Id = id,
                    Word = updateDto.Word?.Trim() ?? string.Empty,
                    Reason = updateDto.Reason?.Trim(),
                    IsActive = updateDto.IsActive,
                    CreatedAt = existingWord.CreatedAt
                };

                // Walidacja przy użyciu wzorca Specification
                var validationResult = ValidateForbiddenWord(forbiddenWord);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                // Sprawdzenie czy słowo już istnieje (z wykluczeniem aktualnego słowa)
                if (await _forbiddenWordRepository.IsWordExistsAsync(forbiddenWord.Word, id))
                {
                    return ApiResponse<ForbiddenWord>.CreateError(
                        $"Zabronione słowo '{forbiddenWord.Word}' już istnieje.", 
                        409 // Konflikt
                    );
                }

                // Aktualizacja w repozytorium
                var updated = await _forbiddenWordRepository.UpdateAsync(id, forbiddenWord);
                if (!updated)
                {
                    return ApiResponse<ForbiddenWord>.CreateError("Nie udało się zaktualizować zakazanego słowa.", 500);
                }

                return ApiResponse<ForbiddenWord>.CreateSuccess(forbiddenWord, "Zabronione słowo zostało zaktualizowane pomyślnie.");
            }
            catch (InvalidOperationException ex)
            {
                // Błąd unikalności z bazy danych
                return ApiResponse<ForbiddenWord>.CreateError(ex.Message, 409);
            }
            catch (Exception ex)
            {
                return ApiResponse<ForbiddenWord>.CreateError(
                    $"Błąd podczas aktualizacji zakazanego słowa: {ex.Message}", 
                    500
                );
            }
        }

        public async Task<ApiResponse> DeleteAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return ApiResponse.CreateErrorResponse("Identyfikator słowa jest wymagany.", 400);
                }

                var deleted = await _forbiddenWordRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse.CreateErrorResponse($"Zabronione słowo o ID {id} nie zostało znalezione.", 404);
                }

                return ApiResponse.CreateSuccessResponse("Zabronione słowo zostało usunięte pomyślnie.");
            }
            catch (Exception ex)
            {
                return ApiResponse.CreateErrorResponse(
                    $"Błąd podczas usuwania zakazanego słowa: {ex.Message}", 
                    500
                );
            }
        }

        public async Task<ApiResponse<ForbiddenWord>> ToggleActiveStatusAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return ApiResponse<ForbiddenWord>.CreateError("Identyfikator słowa jest wymagany.", 400);
                }

                var toggledWord = await _forbiddenWordRepository.ToggleActiveStatusAsync(id);
                if (toggledWord == null)
                {
                    return ApiResponse<ForbiddenWord>.CreateError($"Zabronione słowo o ID {id} nie zostało znalezione.", 404);
                }

                var statusText = toggledWord.IsActive ? "aktywne" : "nieaktywne";
                return ApiResponse<ForbiddenWord>.CreateSuccess(toggledWord, $"Status słowa został zmieniony na {statusText}.");
            }
            catch (Exception ex)
            {
                return ApiResponse<ForbiddenWord>.CreateError(
                    $"Błąd podczas zmiany statusu zakazanego słowa: {ex.Message}", 
                    500
                );
            }
        }

        public async Task<IEnumerable<ForbiddenWord>> GetActiveForbiddenWordsForValidationAsync()
        {
            try
            {
                return await _forbiddenWordRepository.GetActiveAsync();
            }
            catch (Exception)
            {
                // W przypadku błędu zwróć pustą listę - nie blokuj walidacji produktów
                return Enumerable.Empty<ForbiddenWord>();
            }
        }

        /// <summary>
        /// Waliduje zabronione słowo przy użyciu wzorca Specification Pattern.
        /// </summary>
        private ApiResponse<ForbiddenWord> ValidateForbiddenWord(ForbiddenWord forbiddenWord)
        {
            var spec = new ForbiddenWordSpecification();
            var validationErrors = spec.GetValidationErrors(forbiddenWord);

            if (validationErrors.Any())
            {
                return ApiResponse<ForbiddenWord>.CreateValidationError(validationErrors);
            }

            return ApiResponse<ForbiddenWord>.CreateSuccess(forbiddenWord);
        }
    }
}
