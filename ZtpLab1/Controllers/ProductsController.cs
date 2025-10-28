using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Text.RegularExpressions;
using ZtpLab1.Data;
using ZtpLab1.Models;

namespace ZtpLab1.Controllers
{
    /// <summary>
    /// Kontroler obsługujący operacje CRUD na produktach.
    /// Implementuje REST API zgodnie z konwencją.
    /// </summary>
    [ApiController]
    [Route("api/v1/products")] // Wersjonowanie API w ścieżce
    public class ProductsController : ControllerBase
    {
        private readonly IMongoCollection<Product> _products;

        /// <summary>
        /// Konstruktor wykorzystujący Dependency Injection do wstrzyknięcia kontekstu MongoDB.
        /// Unikalność nazw produktów jest wymuszana przez indeks w bazie danych,
        /// a nie przez zapytania w aplikacji - zgodnie z wymaganiami Laboratorium 1.
        /// </summary>
        public ProductsController(MongoDbContext context)
        {
            _products = context.Products;
        }

        /// <summary>
        /// GET /api/v1/products
        /// Pobiera listę wszystkich produktów (tylko Id i Name).
        /// </summary>
        /// <returns>Lista produktów z podstawowymi informacjami</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProducts()
        {
            var products = await _products
                .Find(_ => true)
                .Project(p => new { p.Id, p.Name })
                .ToListAsync();
            return Ok(products);
        }

        /// <summary>
        /// GET /api/v1/products/{id}
        /// Pobiera szczegółowe informacje o pojedynczym produkcie.
        /// </summary>
        /// <param name="id">ID produktu</param>
        /// <returns>Szczegóły produktu lub 404 Not Found</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(string id)
        {
            var product = await _products.Find(p => p.Id == id).FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound($"Produkt o ID {id} nie został znaleziony.");
            }

            return Ok(product);
        }

        /// <summary>
        /// POST /api/v1/products
        /// Tworzy nowy produkt z walidacją domenową.
        /// </summary>
        /// <param name="product">Dane nowego produktu</param>
        /// <returns>201 Created z utworzonym produktem lub odpowiedni kod błędu</returns>
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            try
            {
                // Walidacja domenowa
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!IsPriceValid(product))
                {
                    return BadRequest(new { message = $"Nieprawidłowa cena dla kategorii {product.Category}. " +
                        GetPriceRangeMessage(product.Category) });
                }

                // Próba zapisu do bazy (indeks unikalny na Name wyłapie duplikaty)
                await _products.InsertOneAsync(product);

                // Zwracamy 201 Created z lokalizacją nowego zasobu
                return CreatedAtAction(
                    nameof(GetProduct),
                    new { id = product.Id },
                    product
                );
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
            {
                // Kod 11000 oznacza naruszenie unikalności (duplikat nazwy)
                return Conflict(new { message = $"Produkt o nazwie '{product.Name}' już istnieje." });
            }
        }

        /// <summary>
        /// PUT /api/v1/products/{id}
        /// Aktualizuje istniejący produkt z pełną walidacją domenową.
        /// </summary>
        /// <param name="id">ID produktu do aktualizacji</param>
        /// <param name="product">Nowe dane produktu</param>
        /// <returns>204 No Content lub odpowiedni kod błędu</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, Product product)
        {
            try
            {
                if (id != product.Id)
                {
                    return BadRequest(new { message = "ID w ścieżce nie zgadza się z ID w ciele żądania." });
                }

                // Walidacja domenowa
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!IsPriceValid(product))
                {
                    return BadRequest(new { message = $"Nieprawidłowa cena dla kategorii {product.Category}. " +
                        GetPriceRangeMessage(product.Category) });
                }

                // Sprawdzenie czy produkt istnieje
                var existingProduct = await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
                if (existingProduct == null)
                {
                    return NotFound($"Produkt o ID {id} nie został znaleziony.");
                }

                // Próba aktualizacji (indeks unikalny na Name wyłapie duplikaty)
                var result = await _products.ReplaceOneAsync(p => p.Id == id, product);
                
                return NoContent(); // 204 No Content
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
            {
                // Kod 11000 oznacza naruszenie unikalności (duplikat nazwy)
                return Conflict(new { message = $"Produkt o nazwie '{product.Name}' już istnieje." });
            }
        }

        /// <summary>
        /// DELETE /api/v1/products/{id}
        /// Usuwa produkt z bazy danych.
        /// </summary>
        /// <param name="id">ID produktu do usunięcia</param>
        /// <returns>204 No Content lub 404 Not Found</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var result = await _products.DeleteOneAsync(p => p.Id == id);

            if (result.DeletedCount == 0)
            {
                return NotFound($"Produkt o ID {id} nie został znaleziony.");
            }

            return NoContent(); // 204 No Content
        }

        /// <summary>
        /// Sprawdza czy cena jest prawidłowa dla danej kategorii produktu.
        /// Implementuje reguły biznesowe dla zakresów cen:
        /// - elektronika: 50-50000 PLN
        /// - książki: 5-500 PLN
        /// - odzież: 10-5000 PLN
        /// </summary>
        private bool IsPriceValid(Product product)
        {
            return product.Category?.ToLower() switch
            {
                "elektronika" => product.Price >= 50 && product.Price <= 50000,
                "książki" => product.Price >= 5 && product.Price <= 500,
                "odzież" => product.Price >= 10 && product.Price <= 5000,
                _ => false // Nieznana kategoria = błąd
            };
        }

        /// <summary>
        /// Zwraca komunikat o dozwolonym zakresie cen dla danej kategorii.
        /// </summary>
        private string GetPriceRangeMessage(string category)
        {
            return category?.ToLower() switch
            {
                "elektronika" => "Cena dla elektroniki musi być w zakresie 50-50000 PLN.",
                "książki" => "Cena dla książek musi być w zakresie 5-500 PLN.",
                "odzież" => "Cena dla odzieży musi być w zakresie 10-5000 PLN.",
                _ => "Nieznana kategoria produktu."
            };
        }
    }
}