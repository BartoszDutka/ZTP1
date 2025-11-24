using System.ComponentModel.DataAnnotations;

namespace ZtpLab1.DataTransferObjects
{
    /// <summary>
    /// DTO dla aktualizacji istniejącego produktu.
    /// Używa nowoczesnych wzorców C# 11: 'required' keyword i 'init' accessors.
    /// </summary>
    public class UpdateProductDto
    {
        /// <summary>
        /// Nazwa produktu.
        /// </summary>
        [Required(ErrorMessage = "Nazwa produktu jest wymagana")]
        public required string Name { get; init; }

        /// <summary>
        /// Kategoria produktu.
        /// </summary>
        [Required(ErrorMessage = "Kategoria produktu jest wymagana")]
        public required string Category { get; init; }

        /// <summary>
        /// Cena produktu.
        /// </summary>
        [Required(ErrorMessage = "Cena produktu jest wymagana")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cena musi być większa od 0")]
        public required decimal Price { get; init; }

        /// <summary>
        /// Ilość produktu.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Ilość nie może być ujemna")]
        public int Quantity { get; init; }
    }
}