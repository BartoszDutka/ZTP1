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
        private readonly IMongoCollection<ForbiddenWord> _forbiddenWords;
        private readonly IMongoCollection<ProductHistory> _productHistory;

        /// <summary>
        /// Konstruktor inicjalizujący połączenie z MongoDB i konfigurujący kolekcje.
        /// </summary>
        /// <param name="settings">Ustawienia połączenia z MongoDB</param>
        public MongoDbContext(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _products = database.GetCollection<Product>(settings.ProductsCollectionName);
            _forbiddenWords = database.GetCollection<ForbiddenWord>(settings.ForbiddenWordsCollectionName);
            _productHistory = database.GetCollection<ProductHistory>("ProductHistory");

            // Utworzenie unikalnego indeksu na polu Name przy starcie
            // To wymusza unikalność nazw produktów po stronie bazy danych
            // zgodnie z wymaganiami Laboratorium 1 (ocena 3.0)
            var productIndexKeysDefinition = Builders<Product>.IndexKeys.Ascending(p => p.Name);
            var productIndexOptions = new CreateIndexOptions { Unique = true };
            var productIndexModel = new CreateIndexModel<Product>(productIndexKeysDefinition, productIndexOptions);
            _products.Indexes.CreateOne(productIndexModel);

            // Utworzenie unikalnego indeksu na polu Word w kolekcji zabronionych słów
            // To zapewnia unikalność zabronionych słów (case-insensitive)
            var forbiddenWordIndexKeysDefinition = Builders<ForbiddenWord>.IndexKeys.Ascending(fw => fw.Word);
            var forbiddenWordIndexOptions = new CreateIndexOptions { 
                Unique = true,
                Collation = new Collation("pl", strength: CollationStrength.Primary) // Case-insensitive
            };
            var forbiddenWordIndexModel = new CreateIndexModel<ForbiddenWord>(forbiddenWordIndexKeysDefinition, forbiddenWordIndexOptions);
            _forbiddenWords.Indexes.CreateOne(forbiddenWordIndexModel);

            // Utworzenie indeksów dla kolekcji historii produktów
            // Indeks na ProductId dla szybkiego wyszukiwania historii konkretnego produktu
            var historyProductIdIndexKeysDefinition = Builders<ProductHistory>.IndexKeys.Ascending(h => h.ProductId);
            var historyProductIdIndexModel = new CreateIndexModel<ProductHistory>(historyProductIdIndexKeysDefinition);
            _productHistory.Indexes.CreateOne(historyProductIdIndexModel);

            // Indeks kompozytowy na ProductId i ChangedAt dla szybkiego sortowania historii
            var historyCompositeIndexKeysDefinition = Builders<ProductHistory>.IndexKeys
                .Ascending(h => h.ProductId)
                .Descending(h => h.ChangedAt);
            var historyCompositeIndexModel = new CreateIndexModel<ProductHistory>(historyCompositeIndexKeysDefinition);
            _productHistory.Indexes.CreateOne(historyCompositeIndexModel);
        }

        /// <summary>
        /// Dostęp do kolekcji produktów.
        /// </summary>
        public IMongoCollection<Product> Products => _products;

        /// <summary>
        /// Dostęp do kolekcji zabronionych słów.
        /// </summary>
        public IMongoCollection<ForbiddenWord> ForbiddenWords => _forbiddenWords;

        /// <summary>
        /// Dostęp do kolekcji historii produktów.
        /// </summary>
        public IMongoCollection<ProductHistory> ProductHistory => _productHistory;

        /// <summary>
        /// Sprawdza czy nazwa produktu zawiera zabronione słowa.
        /// </summary>
        /// <param name="productName">Nazwa produktu do sprawdzenia</param>
        /// <returns>Lista znalezionych zabronionych słów lub pusta lista</returns>
        public async Task<List<string>> GetForbiddenWordsInNameAsync(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                return new List<string>();

            // Pobieramy aktywne zabronione słowa z bazy
            var forbiddenWords = await _forbiddenWords
                .Find(fw => fw.IsActive)
                .Project(fw => fw.Word)
                .ToListAsync();

            var foundForbiddenWords = new List<string>();
            var productNameLower = productName.ToLowerInvariant();

            // Sprawdzamy czy nazwa produktu zawiera któreś z zabronionych słów
            foreach (var word in forbiddenWords)
            {
                if (productNameLower.Contains(word.ToLowerInvariant()))
                {
                    foundForbiddenWords.Add(word);
                }
            }

            return foundForbiddenWords;
        }

        /// <summary>
        /// Metoda dodająca przykładowe produkty i zabronione słowa do bazy danych.
        /// Nie sprawdza duplikatów zapytaniem - pozwala bazie wymusić unikalność.
        /// </summary>
        public async Task SeedDataAsync()
        {
            Console.WriteLine("Rozpoczynam seed bazy danych...");
            
            // Seed zabronionych słów
            await SeedForbiddenWordsAsync();
            
            // Seed produktów
            await SeedProductsAsync();
        }

        /// <summary>
        /// Metoda dodająca przykładowe zabronione słowa do bazy danych.
        /// </summary>
        private async Task SeedForbiddenWordsAsync()
        {
            Console.WriteLine("Dodaję zabronione słowa...");
            var forbiddenWords = new[]
            {
                new ForbiddenWord
                {
                    Word = "spam",
                    Reason = "Słowo związane ze spamem",
                    IsActive = true
                },
                new ForbiddenWord
                {
                    Word = "fake",
                    Reason = "Słowo sugerujące fałszywość",
                    IsActive = true
                },
                new ForbiddenWord
                {
                    Word = "hack",
                    Reason = "Słowo związane z nielegalną działalnością",
                    IsActive = true
                },
                new ForbiddenWord
                {
                    Word = "virus",
                    Reason = "Słowo związane z malware",
                    IsActive = true
                },
                new ForbiddenWord
                {
                    Word = "illegal",
                    Reason = "Słowo sugerujące nielegalność",
                    IsActive = true
                }
            };

            foreach (var word in forbiddenWords)
            {
                try
                {
                    Console.WriteLine($"Dodaję zabronione słowo: {word.Word}");
                    await _forbiddenWords.InsertOneAsync(word);
                    Console.WriteLine($"Pomyślnie dodano zabronione słowo: {word.Word}");
                }
                catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
                {
                    Console.WriteLine($"Zabronione słowo {word.Word} już istnieje w bazie");
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd podczas dodawania zakazanego słowa {word.Word}: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Metoda dodająca przykładowe produkty do bazy danych.
        /// Nie sprawdza duplikatów zapytaniem - pozwala bazie wymusić unikalność.
        /// </summary>
        private async Task SeedProductsAsync()
        {
            Console.WriteLine("Dodaję produkty...");
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