using ZtpLab1.Models;
using ZtpLab1.Repositories;
using ZtpLab1.DataTransferObjects;

namespace ZtpLab1.Services
{
    /// <summary>
    /// Implementacja serwisu historii produktów zawierająca logikę biznesową.
    /// Używa Repository Pattern do dostępu do danych historii.
    /// </summary>
    public class ProductHistoryService : IProductHistoryService
    {
        private readonly IProductHistoryRepository _historyRepository;

        public ProductHistoryService(IProductHistoryRepository historyRepository)
        {
            _historyRepository = historyRepository;
        }

        public async Task<ApiResponse<IEnumerable<ProductHistory>>> GetAllHistoryAsync()
        {
            try
            {
                var history = await _historyRepository.GetAllAsync();
                
                return ApiResponse<IEnumerable<ProductHistory>>.SuccessResponse(
                    history, "Pobrano całą historię produktów pomyślnie.");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<ProductHistory>>.CreateError(
                    $"Błąd podczas pobierania historii produktów: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<ProductHistory>>> GetProductHistoryAsync(string productId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productId))
                {
                    return ApiResponse<IEnumerable<ProductHistory>>.CreateError(
                        "Identyfikator produktu jest wymagany.", 400);
                }

                var history = await _historyRepository.GetHistoryByProductIdAsync(productId);
                
                return ApiResponse<IEnumerable<ProductHistory>>.SuccessResponse(
                    history, "Pobrano historię produktu pomyślnie.");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<ProductHistory>>.CreateError(
                    $"Błąd podczas pobierania historii produktu: {ex.Message}", 500);
            }
        }

        public async Task RecordCreateHistoryAsync(Product product)
        {
            try
            {
                var history = new ProductHistory
                {
                    ProductId = product.Id!,
                    OperationType = "CREATE",
                    PreviousState = null,
                    CurrentState = ProductSnapshot.FromProduct(product),
                    ChangedAt = DateTime.UtcNow,
                    ChangedFields = new List<string>()
                };

                await _historyRepository.AddHistoryAsync(history);
            }
            catch (Exception ex)
            {
                // Logowanie błędu - nie przerywamy operacji głównej
                Console.WriteLine($"Błąd podczas zapisywania historii CREATE: {ex.Message}");
            }
        }

        public async Task RecordUpdateHistoryAsync(Product previousProduct, Product currentProduct)
        {
            try
            {
                var changedFields = GetChangedFields(previousProduct, currentProduct);
                
                if (!changedFields.Any())
                {
                    // Brak zmian - nie zapisujemy wpisu historii
                    return;
                }

                var history = new ProductHistory
                {
                    ProductId = currentProduct.Id!,
                    OperationType = "UPDATE",
                    PreviousState = ProductSnapshot.FromProduct(previousProduct),
                    CurrentState = ProductSnapshot.FromProduct(currentProduct),
                    ChangedAt = DateTime.UtcNow,
                    ChangedFields = changedFields
                };

                await _historyRepository.AddHistoryAsync(history);
            }
            catch (Exception ex)
            {
                // Logowanie błędu - nie przerywamy operacji głównej
                Console.WriteLine($"Błąd podczas zapisywania historii UPDATE: {ex.Message}");
            }
        }

        public async Task RecordDeleteHistoryAsync(Product product)
        {
            try
            {
                var history = new ProductHistory
                {
                    ProductId = product.Id!,
                    OperationType = "DELETE",
                    PreviousState = ProductSnapshot.FromProduct(product),
                    CurrentState = null,
                    ChangedAt = DateTime.UtcNow,
                    ChangedFields = new List<string>()
                };

                await _historyRepository.AddHistoryAsync(history);
            }
            catch (Exception ex)
            {
                // Logowanie błędu - nie przerywamy operacji głównej
                Console.WriteLine($"Błąd podczas zapisywania historii DELETE: {ex.Message}");
            }
        }

        /// <summary>
        /// Porównuje dwa produkty i zwraca listę pól, które się zmieniły.
        /// </summary>
        /// <param name="previous">Poprzedni stan produktu</param>
        /// <param name="current">Aktualny stan produktu</param>
        /// <returns>Lista nazw pól, które się zmieniły</returns>
        private List<string> GetChangedFields(Product previous, Product current)
        {
            var changedFields = new List<string>();

            if (previous.Name != current.Name)
                changedFields.Add(nameof(Product.Name));

            if (previous.Category != current.Category)
                changedFields.Add(nameof(Product.Category));

            if (previous.Price != current.Price)
                changedFields.Add(nameof(Product.Price));

            if (previous.Quantity != current.Quantity)
                changedFields.Add(nameof(Product.Quantity));

            return changedFields;
        }
    }
}