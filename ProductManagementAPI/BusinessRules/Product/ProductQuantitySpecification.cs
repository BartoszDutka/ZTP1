using System.ComponentModel.DataAnnotations;
using ZtpLab1.Models;

namespace ZtpLab1.BusinessRules.Product
{
    /// <summary>
    /// Specyfikacja walidacji ilości produktu.
    /// Sprawdza czy ilość nie jest ujemna.
    /// </summary>
    public class ProductQuantitySpecification : BaseSpecification<Models.Product>
    {
        public override string SpecificationName => "ProductQuantity";

        public override IEnumerable<ValidationResult> GetValidationErrors(Models.Product product)
        {
            var errors = new List<ValidationResult>();

            if (product.Quantity < 0)
            {
                errors.Add(new ValidationResult(
                    "Ilość produktu nie może być ujemna.",
                    [nameof(product.Quantity)]
                ));
            }

            return errors;
        }
    }
}