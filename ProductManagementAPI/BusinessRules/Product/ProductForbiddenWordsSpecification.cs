using System.ComponentModel.DataAnnotations;

namespace ZtpLab1.BusinessRules.Product
{
    /// <summary>
    /// Specyfikacja walidacji zabronionych słów w nazwie produktu.
    /// Sprawdza czy nazwa produktu nie zawiera słów z listy zabronionych.
    /// </summary>
    public class ProductForbiddenWordsSpecification : BaseSpecification<Models.Product>
    {
        private readonly IEnumerable<Models.ForbiddenWord> _forbiddenWords;

        public ProductForbiddenWordsSpecification(IEnumerable<Models.ForbiddenWord> forbiddenWords)
        {
            _forbiddenWords = forbiddenWords ?? Enumerable.Empty<Models.ForbiddenWord>();
        }

        public override string SpecificationName => "ProductForbiddenWords";

        public override IEnumerable<ValidationResult> GetValidationErrors(Models.Product product)
        {
            var errors = new List<ValidationResult>();

            if (string.IsNullOrWhiteSpace(product.Name))
            {
                // Jeśli nazwa jest pusta, inne specyfikacje to wykryją
                return errors;
            }

            var productNameLower = product.Name.Trim().ToLowerInvariant();
            var foundForbiddenWords = new List<string>();

            // Sprawdź każde aktywne zabronione słowo
            foreach (var forbiddenWord in _forbiddenWords.Where(fw => fw.IsActive))
            {
                if (productNameLower.Contains(forbiddenWord.Word.ToLowerInvariant()))
                {
                    foundForbiddenWords.Add(forbiddenWord.Word);
                }
            }

            if (foundForbiddenWords.Any())
            {
                var forbiddenWordsText = string.Join(", ", foundForbiddenWords);
                errors.Add(new ValidationResult(
                    $"Naruszenie polityki nazywania produktów. Nazwa zawiera zabronione słowa: {forbiddenWordsText}.",
                    [nameof(product.Name)]
                ));
            }

            return errors;
        }

        /// <summary>
        /// Zwraca listę znalezionych zabronionych słów w nazwie produktu.
        /// </summary>
        public IEnumerable<string> GetForbiddenWordsInName(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                return Enumerable.Empty<string>();

            var productNameLower = productName.Trim().ToLowerInvariant();
            var foundWords = new List<string>();

            foreach (var forbiddenWord in _forbiddenWords.Where(fw => fw.IsActive))
            {
                if (productNameLower.Contains(forbiddenWord.Word.ToLowerInvariant()))
                {
                    foundWords.Add(forbiddenWord.Word);
                }
            }

            return foundWords;
        }
    }
}