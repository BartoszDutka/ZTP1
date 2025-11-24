using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ZtpLab1.Models
{
    /// <summary>
    /// Reprezentuje historię zmian produktu w systemie.
    /// Przechowuje informacje o poprzednim i aktualnym stanie produktu wraz z metadanymi zmiany.
    /// </summary>
    public class ProductHistory
    {
        /// <summary>
        /// Unikalny identyfikator wpisu historii w MongoDB.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// Identyfikator produktu, którego dotyczy historia.
        /// </summary>
        [BsonRequired]
        [Required(ErrorMessage = "Identyfikator produktu jest wymagany")]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// Typ operacji wykonanej na produkcie.
        /// </summary>
        [BsonRequired]
        [Required(ErrorMessage = "Typ operacji jest wymagany")]
        public string OperationType { get; set; } = string.Empty; // UTWORZENIE, AKTUALIZACJA, USUNIĘCIE

        /// <summary>
        /// Stan produktu przed zmianą (null dla operacji CREATE).
        /// </summary>
        public ProductSnapshot? PreviousState { get; set; }

        /// <summary>
        /// Stan produktu po zmianie (null dla operacji DELETE).
        /// </summary>
        public ProductSnapshot? CurrentState { get; set; }

        /// <summary>
        /// Data i czas wykonania zmiany.
        /// </summary>
        [BsonRequired]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Lista pól, które zostały zmienione (tylko dla operacji UPDATE).
        /// </summary>
        public List<string> ChangedFields { get; set; } = new List<string>();
    }

    /// <summary>
    /// Reprezentuje migawkę stanu produktu w określonym momencie.
    /// Używana do przechowywania poprzedniego i aktualnego stanu w historii.
    /// </summary>
    public class ProductSnapshot
    {
        /// <summary>
        /// Nazwa produktu.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Kategoria produktu.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Cena produktu.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Ilość produktu na stanie.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Data utworzenia produktu.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Tworzy migawkę na podstawie obiektu Product.
        /// </summary>
        /// <param name="product">Produkt do skopiowania</param>
        /// <returns>Migawka produktu</returns>
        public static ProductSnapshot FromProduct(Product product)
        {
            return new ProductSnapshot
            {
                Name = product.Name,
                Category = product.Category,
                Price = product.Price,
                Quantity = product.Quantity,
                CreatedAt = product.CreatedAt
            };
        }
    }
}