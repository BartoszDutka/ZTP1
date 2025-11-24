using System.ComponentModel.DataAnnotations;

namespace ZtpLab1.DataTransferObjects
{
    /// <summary>
    /// DTO dla tworzenia nowego zakazanego słowa.
    /// </summary>
    public class CreateForbiddenWordDto
    {
        /// <summary>
        /// Treść zakazanego słowa.
        /// </summary>
        [Required(ErrorMessage = "Zabronione słowo jest wymagane")]
        public required string Word { get; init; }

        /// <summary>
        /// Opis powodu zakazu (opcjonalne).
        /// </summary>
        public string? Reason { get; init; }

        /// <summary>
        /// Czy słowo jest aktywne.
        /// </summary>
        public bool IsActive { get; init; } = true;
    }
}