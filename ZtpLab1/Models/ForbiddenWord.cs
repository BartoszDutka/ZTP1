using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ZtpLab1.Models
{
    /// <summary>
    /// Reprezentuje zabronione słowo w systemie.
    /// Słowa z tej kolekcji nie mogą występować w nazwach produktów.
    /// </summary>
    public class ForbiddenWord
    {
        /// <summary>
        /// Unikalny identyfikator zakazanego słowa w MongoDB.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// Zabronione słowo - unikalne w bazie danych.
        /// Nazwy produktów nie mogą zawierać tego słowa (case-insensitive).
        /// </summary>
        [BsonRequired]
        [Required(ErrorMessage = "Zabronione słowo jest wymagane")]
        [StringLength(50, MinimumLength = 2, 
            ErrorMessage = "Zabronione słowo musi mieć od 2 do 50 znaków")]
        public string Word { get; set; } = string.Empty;

        /// <summary>
        /// Opis przyczyny zakazu (opcjonalne).
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Data dodania słowa do listy zakazanych.
        /// </summary>
        [BsonRequired]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Czy słowo jest aktywne (można czasowo wyłączyć bez usuwania).
        /// </summary>
        [BsonRequired]
        public bool IsActive { get; set; } = true;
    }
}