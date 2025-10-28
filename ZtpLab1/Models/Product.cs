using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ZtpLab1.Models
{
    /// <summary>
    /// Reprezentuje produkt w systemie.
    /// Klasa zawiera podstawowe informacje o produkcie wraz z regułami walidacji.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Unikalny identyfikator produktu w MongoDB.
        /// [BsonId] oznacza, że to pole jest kluczem głównym w MongoDB.
        /// [BsonRepresentation] konwertuje string na ObjectId MongoDB.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        private string _name = string.Empty;

        /// <summary>
        /// Nazwa produktu - unikalna w bazie danych.
        /// Atrybuty zapewniają walidację:
        /// - Required: pole jest wymagane
        /// - StringLength: długość 3-20 znaków
        /// - RegularExpression: tylko litery i cyfry
        /// - BsonRequired: pole wymagane w MongoDB
        /// </summary>
        [BsonRequired] // Wymaga pola w MongoDB
        [Required(ErrorMessage = "Nazwa produktu jest wymagana")]
        [StringLength(20, MinimumLength = 3, 
            ErrorMessage = "Nazwa produktu musi mieć od 3 do 20 znaków")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", 
            ErrorMessage = "Nazwa może zawierać tylko litery i cyfry")]
        public string Name
        {
            get => _name;
            set => _name = value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Kategoria produktu determinująca zakres cen:
        /// - elektronika: 50-50000 PLN
        /// - książki: 5-500 PLN
        /// - odzież: 10-5000 PLN
        /// </summary>
        [BsonRequired]
        [Required(ErrorMessage = "Kategoria produktu jest wymagana")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Cena produktu z walidacją zakresu w zależności od kategorii:
        /// - elektronika: 50-50000 PLN
        /// - książki: 5-500 PLN
        /// - odzież: 10-5000 PLN
        /// Dokładny zakres cen jest walidowany w kontrolerze.
        /// </summary>
        [BsonRequired]
        [Required(ErrorMessage = "Cena produktu jest wymagana")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cena musi być większa od 0")]
        public decimal Price { get; set; }

        /// <summary>
        /// Ilość produktu na stanie.
        /// Walidacja: nie może być ujemna.
        /// </summary>
        [BsonRequired]
        [Range(0, int.MaxValue, ErrorMessage = "Ilość nie może być ujemna")]
        public int Quantity { get; set; }

        /// <summary>
        /// Data utworzenia rekordu produktu.
        /// Ustawiana automatycznie przy tworzeniu nowego produktu.
        /// </summary>
        [BsonRequired]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}