using System.ComponentModel.DataAnnotations;
using ZtpLab1.Models;

namespace ZtpLab1.BusinessRules.ForbiddenWord
{
    /// <summary>
    /// Specyfikacja walidacji zakazanego słowa.
    /// Sprawdza długość i poprawność słowa.
    /// </summary>
    public class ForbiddenWordSpecification : BaseSpecification<Models.ForbiddenWord>
    {
        public override string SpecificationName => "ForbiddenWord";

        public override IEnumerable<ValidationResult> GetValidationErrors(Models.ForbiddenWord forbiddenWord)
        {
            var errors = new List<ValidationResult>();

            // Sprawdzenie czy słowo jest podane
            if (string.IsNullOrWhiteSpace(forbiddenWord.Word))
            {
                errors.Add(new ValidationResult(
                    "Zabronione słowo jest wymagane.",
                    new[] { nameof(forbiddenWord.Word) }
                ));
                return errors;
            }

            var trimmedWord = forbiddenWord.Word.Trim();

            // Sprawdzenie długości słowa
            if (trimmedWord.Length < 2)
            {
                errors.Add(new ValidationResult(
                    "Zabronione słowo musi mieć co najmniej 2 znaki.",
                    new[] { nameof(forbiddenWord.Word) }
                ));
            }
            else if (trimmedWord.Length > 50)
            {
                errors.Add(new ValidationResult(
                    "Zabronione słowo nie może mieć więcej niż 50 znaków.",
                    new[] { nameof(forbiddenWord.Word) }
                ));
            }

            // Sprawdzenie czy słowo nie zawiera tylko białych znaków
            if (string.IsNullOrWhiteSpace(trimmedWord))
            {
                errors.Add(new ValidationResult(
                    "Zabronione słowo nie może składać się tylko z białych znaków.",
                    new[] { nameof(forbiddenWord.Word) }
                ));
            }

            // Opcjonalna walidacja powodu - jeśli podany, sprawdź długość
            if (!string.IsNullOrEmpty(forbiddenWord.Reason) && forbiddenWord.Reason.Length > 500)
            {
                errors.Add(new ValidationResult(
                    "Opis powodu zakazu nie może mieć więcej niż 500 znaków.",
                    new[] { nameof(forbiddenWord.Reason) }
                ));
            }

            return errors;
        }
    }
}