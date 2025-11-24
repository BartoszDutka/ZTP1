using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using ZtpLab1.Models;

namespace ZtpLab1.BusinessRules.Product
{
    /// <summary>
    /// Specyfikacja walidacji nazwy produktu.
    /// Sprawdza długość (3-20 znaków) i zawartość (tylko litery i cyfry).
    /// </summary>
    public class ProductNameSpecification : BaseSpecification<Models.Product>
    {
        public override string SpecificationName => "ProductName";

        public override IEnumerable<ValidationResult> GetValidationErrors(Models.Product product)
        {
            var errors = new List<ValidationResult>();

            // Sprawdzenie czy nazwa jest podana
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                errors.Add(new ValidationResult(
                    "Nazwa produktu jest wymagana.",
                    [nameof(product.Name)]
                ));
                return errors; // Jeśli brak nazwy, dalsze sprawdzenia są bezcelowe
            }

            var trimmedName = product.Name.Trim();

            // Sprawdzenie długości nazwy
            if (trimmedName.Length < 3)
            {
                errors.Add(new ValidationResult(
                    "Nazwa produktu musi mieć co najmniej 3 znaki.",
                    [nameof(product.Name)]
                ));
            }
            else if (trimmedName.Length > 20)
            {
                errors.Add(new ValidationResult(
                    "Nazwa produktu nie może mieć więcej niż 20 znaków.",
                    [nameof(product.Name)]
                ));
            }

            // Sprawdzenie czy zawiera tylko litery i cyfry
            if (!Regex.IsMatch(trimmedName, @"^[a-zA-Z0-9]+$"))
            {
                errors.Add(new ValidationResult(
                    "Nazwa produktu może zawierać tylko litery i cyfry (bez spacji i znaków specjalnych).",
                    [nameof(product.Name)]
                ));
            }

            return errors;
        }
    }
}