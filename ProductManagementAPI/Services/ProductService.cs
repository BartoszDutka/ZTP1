using System.ComponentModel.DataAnnotations;
using ZtpLab1.DataTransferObjects;
using ZtpLab1.Models;
using ZtpLab1.Repositories;
using ZtpLab1.BusinessRules;
using ZtpLab1.BusinessRules.Product;

namespace ZtpLab1.Services
{
    /// <summary>
    /// Implementacja serwisu produktów zawierająca logikę biznesową.
    /// Używa wzorca Specification Pattern do walidacji i Repository Pattern do dostępu do danych.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IForbiddenWordService _forbiddenWordService;
        private readonly IProductHistoryService _historyService;

        public ProductService(IProductRepository productRepository, IForbiddenWordService forbiddenWordService, IProductHistoryService historyService)
        {
            _productRepository = productRepository;
            _forbiddenWordService = forbiddenWordService;
            _historyService = historyService;
        }

        public async Task<ApiResponse<IEnumerable<object>>> GetAllBasicAsync()
        {
            try
            {
                var products = await _productRepository.GetAllBasicInfoAsync();
                return ApiResponse<IEnumerable<object>>.SuccessResponse(products, "Pobrano listę produktów pomyślnie.");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<object>>.ErrorResponse(
                    $"Błąd podczas pobierania listy produktów: {ex.Message}", 
                    500
                );
            }
        }

        public async Task<ApiResponse<Product>> GetByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return ApiResponse<Product>.CreateError("Identyfikator produktu jest wymagany.", 400);
                }

                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    return ApiResponse<Product>.CreateError($"Produkt o ID {id} nie został znaleziony.", 404);
                }

                return ApiResponse<Product>.SuccessResponse(product, "Pobrano szczegóły produktu pomyślnie.");
            }
            catch (Exception ex)
            {
                return ApiResponse<Product>.CreateError(
                    $"Błąd podczas pobierania produktu: {ex.Message}", 
                    500
                );
            }
        }

        public async Task<ApiResponse<Product>> CreateAsync(CreateProductDto createDto)
        {
            try
            {
                // Mapowanie DTO na model
                var product = new Product
                {
                    Name = createDto.Name?.Trim() ?? string.Empty,
                    Category = createDto.Category?.Trim() ?? string.Empty,
                    Price = createDto.Price,
                    Quantity = createDto.Quantity,
                    CreatedAt = DateTime.UtcNow
                };

                // Walidacja przy użyciu wzorca Specification
                var validationResult = await ValidateProductAsync(product, isUpdate: false);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                // Sprawdzenie czy nazwa już istnieje
                if (await _productRepository.IsNameExistsAsync(product.Name))
                {
                    return ApiResponse<Product>.CreateError(
                        $"Produkt o nazwie '{product.Name}' już istnieje.", 
                        409 // Konflikt
                    );
                }

                // Zapis w repozytorium
                var createdProduct = await _productRepository.AddAsync(product);
                
                // Zapisanie historii utworzenia produktu
                await _historyService.RecordCreateHistoryAsync(createdProduct);
                
                return ApiResponse<Product>.SuccessResponse(createdProduct, "Produkt został utworzony pomyślnie.");
            }
            catch (InvalidOperationException ex)
            {
                // Błąd unikalności z bazy danych
                return ApiResponse<Product>.CreateError(ex.Message, 409);
            }
            catch (Exception ex)
            {
                return ApiResponse<Product>.CreateError(
                    $"Błąd podczas tworzenia produktu: {ex.Message}", 
                    500
                );
            }
        }

        public async Task<ApiResponse<Product>> UpdateAsync(string id, UpdateProductDto updateDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return ApiResponse<Product>.CreateError("Identyfikator produktu jest wymagany.", 400);
                }

                // Sprawdzenie czy produkt istnieje
                var existingProduct = await _productRepository.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    return ApiResponse<Product>.CreateError($"Produkt o ID {id} nie został znaleziony.", 404);
                }

                // Mapowanie DTO na model (zachowanie ID i CreatedAt)
                var product = new Product
                {
                    Id = id,
                    Name = updateDto.Name?.Trim() ?? string.Empty,
                    Category = updateDto.Category?.Trim() ?? string.Empty,
                    Price = updateDto.Price,
                    Quantity = updateDto.Quantity,
                    CreatedAt = existingProduct.CreatedAt
                };

                // Walidacja przy użyciu wzorca Specification
                var validationResult = await ValidateProductAsync(product, isUpdate: true, excludeId: id);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                // Sprawdzenie czy nazwa już istnieje (z wykluczeniem aktualnego produktu)
                if (await _productRepository.IsNameExistsAsync(product.Name, id))
                {
                    return ApiResponse<Product>.CreateError(
                        $"Produkt o nazwie '{product.Name}' już istnieje.", 
                        409 // Konflikt
                    );
                }

                // Aktualizacja w repozytorium
                var updated = await _productRepository.UpdateAsync(id, product);
                if (!updated)
                {
                    return ApiResponse<Product>.CreateError("Nie udało się zaktualizować produktu.", 500);
                }

                // Zapisanie historii aktualizacji produktu
                await _historyService.RecordUpdateHistoryAsync(existingProduct, product);

                return ApiResponse<Product>.SuccessResponse(product, "Produkt został zaktualizowany pomyślnie.");
            }
            catch (InvalidOperationException ex)
            {
                // Błąd unikalności z bazy danych
                return ApiResponse<Product>.CreateError(ex.Message, 409);
            }
            catch (Exception ex)
            {
                return ApiResponse<Product>.CreateError(
                    $"Błąd podczas aktualizacji produktu: {ex.Message}", 
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
                    return ApiResponse.CreateErrorResponse("Identyfikator produktu jest wymagany.", 400);
                }

                // Pobierz produkt przed usunięciem dla historii
                var existingProduct = await _productRepository.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    return ApiResponse.CreateErrorResponse($"Produkt o ID {id} nie został znaleziony.", 404);
                }

                var deleted = await _productRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse.CreateErrorResponse("Nie udało się usunąć produktu.", 500);
                }

                // Zapisanie historii usunięcia produktu
                await _historyService.RecordDeleteHistoryAsync(existingProduct);

                return ApiResponse.SuccessResponse("Produkt został usunięty pomyślnie.");
            }
            catch (Exception ex)
            {
                return ApiResponse.CreateErrorResponse(
                    $"Błąd podczas usuwania produktu: {ex.Message}", 
                    500
                );
            }
        }

        /// <summary>
        /// Waliduje produkt przy użyciu wzorca Specification Pattern.
        /// </summary>
        private async Task<ApiResponse<Product>> ValidateProductAsync(Product product, bool isUpdate, string? excludeId = null)
        {
            var validationErrors = new List<ValidationResult>();

            // Specyfikacje walidacji
            var nameSpec = new ProductNameSpecification();
            var priceSpec = new ProductPriceSpecification();
            var quantitySpec = new ProductQuantitySpecification();

            // Dodaj błędy z każdej specyfikacji
            validationErrors.AddRange(nameSpec.GetValidationErrors(product));
            validationErrors.AddRange(priceSpec.GetValidationErrors(product));
            validationErrors.AddRange(quantitySpec.GetValidationErrors(product));

            // Specyfikacja zabronionych słów (tylko jeśli nazwa jest prawidłowa)
            if (!validationErrors.Any(ve => ve.MemberNames.Contains(nameof(product.Name))))
            {
                var forbiddenWords = await _forbiddenWordService.GetActiveForbiddenWordsForValidationAsync();
                var forbiddenWordsSpec = new ProductForbiddenWordsSpecification(forbiddenWords);
                validationErrors.AddRange(forbiddenWordsSpec.GetValidationErrors(product));
            }

            if (validationErrors.Any())
            {
                return ApiResponse<Product>.CreateValidationError(validationErrors);
            }

            return ApiResponse<Product>.SuccessResponse(product);
        }
    }
}
