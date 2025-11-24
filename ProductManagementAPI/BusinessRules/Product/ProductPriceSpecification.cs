using System.ComponentModel.DataAnnotations;
using ZtpLab1.Models;

namespace ZtpLab1.BusinessRules.Product
{
    /// <summary>
    /// Specyfikacja walidacji ceny produktu według kategorii.
    /// Sprawdza czy cena mieści się w dozwolonym zakresie dla danej kategorii.
    /// </summary>
    public class ProductPriceSpecification : BaseSpecification<Models.Product>
    {
        public override string SpecificationName => "ProductPrice";

        private readonly Dictionary<string, (decimal Min, decimal Max)> _categoryPriceRanges = new()
        {
            { "elektronika", (50m, 50000m) },
            { "książki", (5m, 500m) },
            { "odzież", (10m, 5000m) }
        };

        public override IEnumerable<ValidationResult> GetValidationErrors(Models.Product product)
        {
            var errors = new List<ValidationResult>();

            // Sprawdzenie czy kategoria jest podana
            if (string.IsNullOrWhiteSpace(product.Category))
            {
                errors.Add(new ValidationResult(
                    "Kategoria produktu jest wymagana.",
                    [nameof(product.Category)]
                ));
                return errors;
            }

            // Sprawdzenie czy cena jest dodatnia
            if (product.Price <= 0)
            {
                errors.Add(new ValidationResult(
                    "Cena produktu musi być większa od 0.",
                    [nameof(product.Price)]
                ));
            }

            // Sprawdzenie zakresu ceny dla kategorii
            var categoryLower = product.Category.Trim().ToLowerInvariant();
            
            if (_categoryPriceRanges.TryGetValue(categoryLower, out var priceRange))
            {
                if (product.Price < priceRange.Min)
                {
                    errors.Add(new ValidationResult(
                        $"Cena produktu w kategorii '{product.Category}' nie może być mniejsza niż {priceRange.Min:F2} PLN.",
                        [nameof(product.Price)]
                    ));
                }
                else if (product.Price > priceRange.Max)
                {
                    errors.Add(new ValidationResult(
                        $"Cena produktu w kategorii '{product.Category}' nie może być większa niż {priceRange.Max:F2} PLN.",
                        [nameof(product.Price)]
                    ));
                }
            }
            else
            {
                errors.Add(new ValidationResult(
                    $"Nieznana kategoria produktu: '{product.Category}'. Dostępne kategorie: {string.Join(", ", _categoryPriceRanges.Keys)}.",
                    [nameof(product.Category)]
                ));
            }

            return errors;
        }

        /// <summary>
        /// Zwraca informację o dozwolonym zakresie cen dla kategorii.
        /// </summary>
        public string GetPriceRangeInfo(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return "Nieznana kategoria.";

            var categoryLower = category.Trim().ToLowerInvariant();
            
            if (_categoryPriceRanges.TryGetValue(categoryLower, out var range))
            {
                return $"Dozwolony zakres cen dla kategorii '{category}': {range.Min:F2} - {range.Max:F2} PLN.";
            }
            
            return $"Nieznana kategoria: '{category}'. Dostępne kategorie: {string.Join(", ", _categoryPriceRanges.Keys)}.";
        }
    }
}