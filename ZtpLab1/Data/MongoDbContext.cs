using MongoDB.Driver;
using ZtpLab1.Models;

namespace ZtpLab1.Data
{
    /// <summary>
    /// Klasa kontekstu MongoDB odpowiedzialna za połączenie z bazą danych
    /// i konfigurację kolekcji.
    /// </summary>
    public class MongoDbContext
    {
        private readonly IMongoCollection<Product> _products;

        /// <summary>
        /// Konstruktor inicjalizujący połączenie z MongoDB i konfigurujący kolekcje.
        /// </summary>
        /// <param name="settings">Ustawienia połączenia z MongoDB</param>
        public MongoDbContext(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _products = database.GetCollection<Product>(settings.ProductsCollectionName);

            // Utworzenie unikalnego indeksu na polu Name przy starcie
            // To wymusza unikalność nazw produktów po stronie bazy danych
            // zgodnie z wymaganiami Laboratorium 1 (ocena 3.0)
            var indexKeysDefinition = Builders<Product>.IndexKeys.Ascending(p => p.Name);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<Product>(indexKeysDefinition, indexOptions);
            _products.Indexes.CreateOne(indexModel);
        }

        /// <summary>
        /// Dostęp do kolekcji produktów.
        /// </summary>
        public IMongoCollection<Product> Products => _products;

        /// <summary>
        /// Metoda dodająca przykładowe produkty do bazy danych.
        /// Nie sprawdza duplikatów zapytaniem - pozwala bazie wymusić unikalność.
        /// </summary>
        public async Task SeedProductsAsync()
        {
            Console.WriteLine("Rozpoczynam seed bazy danych...");
            var products = new[]
            {
                new Product
                {
                    Name = "LaptopAcer",
                    Category = "elektronika",
                    Price = 4500,
                    Quantity = 10
                },
                new Product
                {
                    Name = "KoszulkaNike",
                    Category = "odzież",
                    Price = 120,
                    Quantity = 50
                },
                new Product
                {
                    Name = "HarryPotter",
                    Category = "książki",
                    Price = 45,
                    Quantity = 100
                },
                new Product
                {
                    Name = "SmartfonSamsung",
                    Category = "elektronika",
                    Price = 3200,
                    Quantity = 25
                },
                new Product
                {
                    Name = "BluzaAdidas",
                    Category = "odzież",
                    Price = 230,
                    Quantity = 40
                }
            };

            foreach (var product in products)
            {
                try
                {
                    // Próbujemy dodać produkt - baza sama wyłapie duplikaty
                    Console.WriteLine($"Dodaję produkt: {product.Name}");
                    await _products.InsertOneAsync(product);
                    Console.WriteLine($"Pomyślnie dodano produkt: {product.Name}");
                }
                catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
                {
                    // Ignorujemy duplikaty (code 11000) - to oczekiwane zachowanie
                    Console.WriteLine($"Produkt {product.Name} już istnieje w bazie");
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd podczas dodawania produktu {product.Name}: {ex.Message}");
                    throw;
                }
            }
        }
    }
}