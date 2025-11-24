# 📖 Dokumentacja - System Zarządzania Produktami

## 🎯 Spis treści
1. [Jak działa cała aplikacja - krok po kroku](#jak-działa-cała-aplikacja---krok-po-kroku)
2. [Zasady działania - co się dzieje gdy...](#zasady-działania---co-się-dzieje-gdy)
3. [Spełnienie wymagań laboratorium](#spełnienie-wymagań-laboratorium)
4. [Architektura i komunikacja](#architektura-i-komunikacja)
5. [Modele danych - jak są zbudowane](#modele-danych---jak-są-zbudowane)
6. [Kontrolery - logika biznesowa](#kontrolery---logika-biznesowa)
7. [MongoDB - jak działa baza danych](#mongodb---jak-działa-baza-danych)
8. [System walidacji - jak chronimy dane](#system-walidacji---jak-chronimy-dane)
9. [System zabronionych słów - jak blokujemy nazwy](#system-zabronionych-słów---jak-blokujemy-nazwy)
10. [Testowanie - jak sprawdzić że działa](#testowanie---jak-sprawdzić-że-działa)

---

## 🚀 Jak działa cała aplikacja - krok po kroku

### **Wyobraź sobie że jesteś klientem i chcesz dodać nowy produkt do sklepu:**

1. **Otwierasz aplikację** - wpisujesz `http://localhost:5048` w przeglądarce
2. **Widzisz Swagger UI** - to taka ładna strona gdzie możesz testować API
3. **Klikasz na "POST /api/v1/products"** - to znaczy "dodaj nowy produkt"
4. **Wpisujesz dane produktu** - nazwę, kategorię, cenę, ilość
5. **Klikasz "Execute"** - wysyłasz żądanie do serwera

### **Co się dzieje po kliknięciu "Execute"? (To jest magia! ✨)**

**KROK 1: Żądanie HTTP leci do serwera**
```
Twoja przeglądarka → HTTP POST → Serwer ASP.NET Core
```
Żądanie zawiera JSON z danymi produktu, np.:
```json
{
  "name": "LaptopDell", 
  "category": "elektronika",
  "price": 3500,
  "quantity": 5
}
```

**KROK 2: ASP.NET Core odbiera żądanie**
- Framework automatycznie "rozpakuje" JSON i utworzy obiekt typu `Product`
- To nazywa się "model binding" - JSON zamienia się na obiekt C#

**KROK 3: Kontroler ProductsController zaczyna działać**
- Metoda `CreateProduct()` otrzymuje gotowy obiekt Product
- Teraz zaczynają się wszystkie sprawdzenia...

**KROK 4: Pierwsze sprawdzenie - czy dane są poprawne?**
```csharp
if (!ModelState.IsValid)
    return BadRequest(ModelState);
```
Tu sprawdza się:
- Czy nazwa ma 3-20 znaków? ✅
- Czy zawiera tylko litery i cyfry? ✅  
- Czy cena jest większa od 0? ✅

**KROK 5: Drugie sprawdzenie - czy cena pasuje do kategorii?**
```csharp
if (!IsPriceValid(product))
    return BadRequest("Zła cena!");
```
- Elektronika: 50-50000 PLN → 3500 PLN ✅ OKEJ
- Gdyby była książka za 3500 PLN → ❌ BŁĄD (max 500 PLN)

**KROK 6: Trzecie sprawdzenie - czy nazwa zawiera zabronione słowa?**
```csharp
var forbiddenWords = await GetForbiddenWordsInNameAsync(product.Name);
if (forbiddenWords.Any())
    return BadRequest("Zabronione słowo!");
```
- Sprawdza czy "LaptopDell" zawiera "spam", "fake", "virus" itp.
- W naszym przypadku: ✅ OKEJ

**KROK 7: Zapis do bazy MongoDB**
```csharp
await _products.InsertOneAsync(product);
```
- Jeśli wszystko OK → produkt ląduje w bazie
- Jeśli nazwa już istnieje → MongoDB rzuca błąd (duplikat!)

**KROK 8: Odpowiedź do klienta**
```
Serwer ASP.NET Core → HTTP 201 Created → Twoja przeglądarka
```
- Status 201 = "Utworzono pomyślnie"
- W odpowiedzi: pełne dane produktu + jego nowe ID

### **A co jeśli coś pójdzie nie tak?**

**Scenariusz 1: Zła nazwa**
```json
{"name": "x1"} // Za krótka nazwa (min 3 znaki)
```
**Rezultat:** HTTP 400 + komunikat "Nazwa musi mieć 3-20 znaków"

**Scenariusz 2: Zła cena**
```json
{"name": "Laptop123", "category": "książki", "price": 3000}
```
**Rezultat:** HTTP 400 + "Cena dla książek max 500 PLN"

**Scenariusz 3: Zabronione słowo**
```json
{"name": "SpamLaptop"}
```
**Rezultat:** HTTP 400 + "Naruszenie polityki nazywania produktów"

**Scenariusz 4: Duplikat nazwy**
```json
{"name": "LaptopDell"} // Już istnieje!
```
**Rezultat:** HTTP 409 Conflict + "Produkt już istnieje"

---

## 🎭 Zasady działania - co się dzieje gdy...

### **🔍 Gdy pobierasz LISTĘ produktów (GET /api/v1/products):**

**Twoje żądanie:**
```
GET http://localhost:5048/api/v1/products
```

**Co się dzieje w aplikacji:**
1. Kontroler wykonuje: `_products.Find(_ => true).Project(p => new { p.Id, p.Name })`
2. MongoDB zwraca WSZYSTKIE produkty, ale tylko pola Id i Name
3. Dostajesz odpowiedź typu:
```json
[
  {"id": "507f1f77bcf86cd799439011", "name": "LaptopAcer"},
  {"id": "507f1f77bcf86cd799439012", "name": "KoszulkaNike"}
]
```

**Dlaczego tylko Id i Name?** - To wymaganie laboratorium! Lista ma być "lekka".

### **🔎 Gdy pobierasz SZCZEGÓŁY produktu (GET /api/v1/products/{id}):**

**Twoje żądanie:**
```
GET http://localhost:5048/api/v1/products/507f1f77bcf86cd799439011
```

**Co się dzieje:**
1. Kontroler szuka produktu: `_products.Find(p => p.Id == id)`
2. Jeśli znajdzie → zwraca WSZYSTKIE dane (w tym quantity!)
3. Jeśli nie znajdzie → HTTP 404 Not Found

**Otrzymujesz:**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "name": "LaptopAcer", 
  "category": "elektronika",
  "price": 4500,
  "quantity": 10,  // ← To jest wymaganie laboratorium!
  "createdAt": "2024-11-17T10:30:00Z"
}
```

### **✏️ Gdy edytujesz produkt (PUT /api/v1/products/{id}):**

**To jest prawie to samo co tworzenie, ale:**
1. Sprawdza czy ID w URL = ID w danych
2. Sprawdza czy produkt istnieje
3. Wszystkie te same walidacje (cena, zabronione słowa)
4. MongoDB zastępuje stary produkt nowym: `ReplaceOneAsync()`

### **🗑️ Gdy usuwasz produkt (DELETE /api/v1/products/{id}):**

**Bardzo proste:**
1. MongoDB usuwa: `DeleteOneAsync(p => p.Id == id)`
2. Jeśli usunął (DeletedCount = 1) → HTTP 204 No Content
3. Jeśli nie znalazł → HTTP 404 Not Found

---

## 🏗️ Architektura i komunikacja

### **Jak komponenty ze sobą rozmawiają:**

```
🌐 Przeglądarka/Postman
          │ HTTP Request (JSON)
          ▼
🎯 ASP.NET Core Controller  
          │ 
          ├─→ 📊 Model (Product/ForbiddenWord) - struktura danych
          │
          ├─→ ✅ Validation Attributes - sprawdzenie danych
          │
          ├─→ 💾 MongoDbContext - komunikacja z bazą
          │         │
          │         ▼
          │    🗃️ MongoDB Collection
          │         │
          │         ▼
          │    📄 MongoDB Document (JSON w bazie)
          │
          ▼
🌐 HTTP Response (JSON/Error)
```

### **Dlaczego taka architektura?**

1. **Separation of Concerns** - każda klasa ma swoją odpowiedzialność
2. **Models** - opisują jak wyglądają dane (Product, ForbiddenWord)
3. **Controllers** - zawierają logikę biznesową (walidacja, CRUD)
4. **Data Layer** - komunikacja z bazą danych
5. **MongoDB** - przechowywanie danych

### **Przepływ danych krok po kroku:**

```
JSON z przeglądarki 
    ↓ (Model Binding)
Obiekt C# (Product)
    ↓ (Validation)
Sprawdzenie poprawności
    ↓ (Business Logic) 
Logika biznesowa (ceny, zabronione słowa)
    ↓ (Data Access)
Zapis do MongoDB
    ↓ (Serialization)
JSON Document w bazie
```

---

## ✅ Spełnienie wymagań laboratorium

### 🏆 **OCENA 3.0 - Pełne spełnienie**

| Wymaganie | Implementacja | Status |
|-----------|---------------|--------|
| **GET /api/v1/products** | `ProductsController.GetProducts()` | ✅ |
| **GET /api/v1/products/{id}** | `ProductsController.GetProduct(id)` | ✅ |
| **POST /api/v1/products** | `ProductsController.CreateProduct()` | ✅ |
| **PUT /api/v1/products/{id}** | `ProductsController.UpdateProduct()` | ✅ |
| **DELETE /api/v1/products/{id}** | `ProductsController.DeleteProduct()` | ✅ |
| **Nazwa unikalna (3-20 znaków)** | Atrybuty walidacji + indeks MongoDB | ✅ |
| **Tylko litery i cyfry** | `RegularExpression(@"^[a-zA-Z0-9]+$")` | ✅ |
| **Ceny według kategorii** | Metoda `IsPriceValid()` | ✅ |
| **Baza danych** | MongoDB z kolekcją Products | ✅ |

### 🏆 **OCENA 3.5 - Pełne spełnienie**

| Wymaganie | Implementacja | Status |
|-----------|---------------|--------|
| **Zabronione słowa w bazie** | Kolekcja `ForbiddenWords` | ✅ |
| **Walidacja przy dodawaniu** | `GetForbiddenWordsInNameAsync()` w POST | ✅ |
| **Walidacja przy edycji** | `GetForbiddenWordsInNameAsync()` w PUT | ✅ |
| **Komunikat o naruszeniu** | "Naruszenie polityki nazywania produktów" | ✅ |

---

## 📁 Struktura projektu

```
ZtpLab1/
├── Controllers/
│   ├── ProductsController.cs       # API dla produktów
│   └── ForbiddenWordsController.cs # API dla zabronionych słów
├── Models/
│   ├── Product.cs                  # Model produktu
│   └── ForbiddenWord.cs            # Model zakazanego słowa
├── Data/
│   ├── MongoDbContext.cs           # Kontekst bazy danych
│   └── MongoDbSettings.cs          # Konfiguracja MongoDB
├── Program.cs                      # Konfiguracja aplikacji
└── appsettings.json               # Ustawienia (connection string)
```

---

## 📊 Modele danych - jak są zbudowane

### 🛍️ **Product - czyli jak wygląda produkt w naszym systemie**

**Wyobraź sobie produkt jak kartę produktu w sklepie internetowym:**

```csharp
public class Product
{
    [BsonId] // To jak kod kreskowy - unikalny identyfikator
    public string? Id { get; set; }
    
    [Required] // MUSI być podane
    [StringLength(20, MinimumLength = 3)] // 3-20 znaków
    [RegularExpression(@"^[a-zA-Z0-9]+$")] // Tylko litery i cyfry
    public string Name { get; set; } // Nazwa produktu
    
    [Required] // MUSI być podane
    public string Category { get; set; } // elektronika/książki/odzież
    
    [Required] // MUSI być podane
    [Range(0.01, double.MaxValue)] // Musi być > 0
    public decimal Price { get; set; } // Cena w PLN
    
    [Range(0, int.MaxValue)] // Nie może być ujemna
    public int Quantity { get; set; } // Ile sztuk na stanie
    
    public DateTime CreatedAt { get; set; } // Kiedy dodano do bazy
}
```

### **Co oznaczają te "dziwne" rzeczy w nawiasach kwadratowych?**

**[BsonId]** - mówi MongoDB: "to jest klucz główny"
- Jak numer dowodu osobistego - każdy produkt ma unikalny
- MongoDB automatycznie generuje, np. "507f1f77bcf86cd799439011"

**[Required]** - pole OBOWIĄZKOWE
- Jeśli nie podasz → błąd "pole wymagane"
- ASP.NET sprawdza to automatycznie

**[StringLength(20, MinimumLength = 3)]** - długość tekstu
- Minimum 3 znaki, maksimum 20
- "ab" → ❌ za krótkie
- "NazwaProduktu123" → ✅ OK
- "NazwaProduktuBardzoDluga123456" → ❌ za długie

**[RegularExpression(@"^[a-zA-Z0-9]+$")]** - tylko litery i cyfry
- "Laptop123" → ✅ OK
- "Laptop-Dell" → ❌ ma myślnik
- "Laptop Dell" → ❌ ma spację
- "Laptop!" → ❌ ma wykrzyknik

**[Range(0.01, double.MaxValue)]** - zakres liczb
- Cena musi być większa niż 0
- 0 → ❌ za mało
- 0.01 → ✅ OK (1 grosz)
- 999999 → ✅ OK

### **Przykład produktu w bazie danych (MongoDB Document):**
```json
{
  "_id": "507f1f77bcf86cd799439011",
  "name": "LaptopDell5500",
  "category": "elektronika", 
  "price": 4500.00,
  "quantity": 15,
  "createdAt": "2024-11-17T10:30:00.000Z"
}
```

### **Dlaczego tak, a nie inaczej?**

1. **Name jest unikalny** - nie może być dwóch produktów o tej samej nazwie
2. **Category determinuje ceny** - każda kategoria ma swój zakres cenowy
3. **Price jest decimal** - żeby obsłużyć grosze (4500.99 PLN)
4. **Quantity może być 0** - produkt może być chwilowo wyprzedany
5. **CreatedAt** - żeby wiedzieć kiedy produkt został dodany

### 🚫 **ForbiddenWord - czyli lista "zabronionych słów"**

**To jak lista brzydkich słów w moderacji komentarzy na forum:**

```csharp
public class ForbiddenWord
{
    [BsonId] // Unikalny ID w bazie
    public string? Id { get; set; }
    
    [Required] // MUSI być podane
    [StringLength(50, MinimumLength = 2)] // 2-50 znaków
    public string Word { get; set; } // Zakazane słowo
    
    public string? Reason { get; set; } // Dlaczego zakazane (opcjonalne)
    
    public DateTime CreatedAt { get; set; } // Kiedy dodano do listy
    
    public bool IsActive { get; set; } = true; // Czy aktualnie blokuje
}
```

### **Jak to działa w praktyce?**

**Przykładowe zabronione słowa w bazie:**
```json
[
  {
    "_id": "507f1f77bcf86cd799439020",
    "word": "spam",
    "reason": "Słowo związane ze spamem", 
    "createdAt": "2024-11-17T08:00:00.000Z",
    "isActive": true
  },
  {
    "_id": "507f1f77bcf86cd799439021", 
    "word": "fake",
    "reason": "Słowo sugerujące fałszywość",
    "createdAt": "2024-11-17T08:01:00.000Z", 
    "isActive": true
  },
  {
    "_id": "507f1f77bcf86cd799439022",
    "word": "test", 
    "reason": "Słowo testowe",
    "createdAt": "2024-11-17T09:00:00.000Z",
    "isActive": false  // WYŁĄCZONE - nie blokuje
  }
]
```

### **Proces blokowania nazw produktów:**

1. **Ktoś próbuje dodać produkt "SpamLaptop"**
2. **System pobiera aktywne zabronione słowa:** ["spam", "fake"] (bez "test" bo wyłączony)
3. **Sprawdza czy nazwa zawiera któreś słowo:**
   - "spamlaptop".contains("spam") → ✅ ZNALAZŁ!
   - "spamlaptop".contains("fake") → ❌ nie ma
4. **Rezultat:** Błąd "Naruszenie polityki nazywania produktów"

### **IsActive - po co to pole?**
- `true` - słowo aktywnie blokuje nazwy produktów
- `false` - słowo wyłączone (można je później włączyć bez usuwania)
- **Dlaczego tak?** Czasem chcemy tymczasowo wyłączyć słowo bez jego usuwania

### **Przykłady użycia:**
```
"SpamProduct123" → ❌ BŁĄD (zawiera "spam")
"FakeLaptop" → ❌ BŁĄD (zawiera "fake") 
"TestLaptop" → ✅ OK (słowo "test" jest wyłączone)
"SuperLaptop" → ✅ OK (nie zawiera zabronionych słów)
```

---

## 🎯 Kontrolery - logika biznesowa

### **Co to jest kontroler?**
Kontroler to jak recepcjonista w hotelu - odbiera żądania od klientów i załatwia sprawy. 

- **Klient mówi:** "Chcę zobaczyć listę produktów"  
- **Kontroler robi:** Pyta bazę danych, dostaje listę, oddaje klientowi
- **Klient mówi:** "Chcę dodać nowy produkt"
- **Kontroler robi:** Sprawdza czy dane są OK, zapisuje w bazie, potwierdza

### 🛍️ **ProductsController** - główny "recepcjonista" dla produktów

**Ten kontroler obsługuje 5 operacji (CRUD + Read details):**

#### **1. GET /api/v1/products** - "Pokaż mi listę wszystkich produktów"

**Jak to działa krok po kroku:**
```csharp
public async Task<ActionResult<IEnumerable<object>>> GetProducts()
{
    // 1. Znajdź WSZYSTKIE produkty w bazie
    // Find(_ => true) oznacza "bez filtrów, weź wszystko"
    
    // 2. Ale weź tylko Id i Name (nie cały produkt!)
    // Project() to jak "SELECT id, name FROM products" w SQL
    
    // 3. Zamień na listę
    var products = await _products
        .Find(_ => true)           // Wszystkie produkty
        .Project(p => new { p.Id, p.Name })  // Tylko Id i Name
        .ToListAsync();            // Zrób z tego listę
        
    // 4. Zwróć wynik jako JSON
    return Ok(products);
}
```

**Dlaczego tylko Id i Name?**
- To wymaganie laboratorium - lista ma być "lekka"
- Szczegóły dostaniesz dopiero gdy klikniesz na konkretny produkt

**Przykład odpowiedzi:**
```json
[
  {"id": "507f1f77bcf86cd799439011", "name": "LaptopAcer"},
  {"id": "507f1f77bcf86cd799439012", "name": "KoszulkaNike"}, 
  {"id": "507f1f77bcf86cd799439013", "name": "HarryPotter"}
]
```

#### **2. GET /api/v1/products/{id}** - "Pokaż mi szczegóły tego produktu"

**Przykład: GET /api/v1/products/507f1f77bcf86cd799439011**

```csharp
public async Task<ActionResult<Product>> GetProduct(string id)
{
    // 1. Szukaj produktu po ID
    var product = await _products
        .Find(p => p.Id == id)        // Znajdź gdzie Id = podane ID
        .FirstOrDefaultAsync();       // Weź pierwszy lub null
    
    // 2. Sprawdź czy znalazł
    if (product == null)
    {
        // HTTP 404 Not Found
        return NotFound($"Produkt o ID {id} nie został znaleziony.");
    }
        
    // 3. Zwróć WSZYSTKIE dane produktu
    return Ok(product);
}
```

**Co dostajesz w odpowiedzi (wszystkie szczegóły!):**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "name": "LaptopAcer",
  "category": "elektronika", 
  "price": 4500.00,
  "quantity": 15,  // ← WAŻNE! To wymaganie laboratorium
  "createdAt": "2024-11-17T10:30:00.000Z"
}
```

**A co jeśli podasz złe ID?**
```
GET /api/v1/products/nieistniejace-id
→ HTTP 404 + "Produkt o ID nieistniejace-id nie został znaleziony."
```

#### **POST /api/v1/products** - Tworzenie produktu
```csharp
public async Task<ActionResult<Product>> CreateProduct(Product product)
{
    // 1. Walidacja modelu (atrybuty)
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    
    // 2. Walidacja ceny według kategorii
    if (!IsPriceValid(product))
        return BadRequest("Nieprawidłowa cena dla kategorii");
    
    // 3. Sprawdzenie zabronionych słów (OCENA 3.5)
    var forbiddenWords = await _context.GetForbiddenWordsInNameAsync(product.Name);
    if (forbiddenWords.Any())
        return BadRequest("Naruszenie polityki nazywania produktów");
    
    // 4. Zapis do bazy (MongoDB wyłapie duplikaty nazw)
    await _products.InsertOneAsync(product);
    
    return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
}
```

#### **PUT /api/v1/products/{id}** - Aktualizacja produktu
```csharp
public async Task<IActionResult> UpdateProduct(string id, Product product)
{
    // Identyczna walidacja jak przy tworzeniu
    // + sprawdzenie czy produkt istnieje
    // + sprawdzenie zgodności ID
}
```

#### **DELETE /api/v1/products/{id}** - Usuwanie produktu
```csharp
public async Task<IActionResult> DeleteProduct(string id)
{
    var result = await _products.DeleteOneAsync(p => p.Id == id);
    
    if (result.DeletedCount == 0)
        return NotFound($"Produkt o ID {id} nie został znaleziony.");
        
    return NoContent(); // 204 No Content
}
```

### 🚫 **ForbiddenWordsController** - Zarządzanie zabronionymi słowami

Pełne CRUD dla zabronionych słów:
- **GET /api/v1/forbidden-words** - lista wszystkich
- **GET /api/v1/forbidden-words/active** - tylko aktywne
- **POST /api/v1/forbidden-words** - dodaj nowe
- **PUT /api/v1/forbidden-words/{id}** - edytuj
- **DELETE /api/v1/forbidden-words/{id}** - usuń
- **PATCH /api/v1/forbidden-words/{id}/toggle** - włącz/wyłącz

---

## 💾 Baza danych MongoDB

### **MongoDbContext** - Kontekst bazy danych

```csharp
public class MongoDbContext
{
    private readonly IMongoCollection<Product> _products;
    private readonly IMongoCollection<ForbiddenWord> _forbiddenWords;
    
    public MongoDbContext(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        
        _products = database.GetCollection<Product>("Products");
        _forbiddenWords = database.GetCollection<ForbiddenWord>("ForbiddenWords");
        
        // INDEKSY UNIKALNE - wymuszają unikalność po stronie bazy
        CreateUniqueIndexes();
    }
}
```

### **Indeksy unikalne**
```csharp
// Unikalność nazw produktów
var productIndex = Builders<Product>.IndexKeys.Ascending(p => p.Name);
var productOptions = new CreateIndexOptions { Unique = true };
_products.Indexes.CreateOne(new CreateIndexModel<Product>(productIndex, productOptions));

// Unikalność zabronionych słów
var wordIndex = Builders<ForbiddenWord>.IndexKeys.Ascending(fw => fw.Word);
var wordOptions = new CreateIndexOptions { 
    Unique = true,
    Collation = new Collation("pl", strength: CollationStrength.Primary) // Case-insensitive
};
_forbiddenWords.Indexes.CreateOne(new CreateIndexModel<ForbiddenWord>(wordIndex, wordOptions));
```

### **Konfiguracja połączenia** (appsettings.json)
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb+srv://user:password@cluster.mongodb.net/",
    "DatabaseName": "ZtpLab1Db",
    "ProductsCollectionName": "Products",
    "ForbiddenWordsCollectionName": "ForbiddenWords"
  }
}
```

---

## ✔️ Walidacja domenowa

### 1. **Walidacja nazwy produktu**

```csharp
[Required(ErrorMessage = "Nazwa produktu jest wymagana")]
[StringLength(20, MinimumLength = 3, ErrorMessage = "Nazwa musi mieć 3-20 znaków")]
[RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Tylko litery i cyfry")]
public string Name { get; set; }
```

**Jak działa:**
- ASP.NET Core automatycznie waliduje przed wejściem do kontrolera
- Jeśli walidacja nie przejdzie → `ModelState.IsValid = false`
- Kontroler zwraca `BadRequest(ModelState)` z szczegółami błędów

### 2. **Walidacja unikalności nazw**

```csharp
try {
    await _products.InsertOneAsync(product);
} catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000) {
    // Kod 11000 = naruszenie unikalności
    return Conflict(new { message = $"Produkt o nazwie '{product.Name}' już istnieje." });
}
```

**Jak działa:**
- MongoDB ma unikalny indeks na polu `Name`
- Przy próbie dodania duplikatu → MongoDB rzuca wyjątek z kodem 11000
- Aplikacja łapie wyjątek i zwraca 409 Conflict

### 3. **Walidacja ceny według kategorii**

```csharp
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
```

**Zasady cenowe:**
- **Elektronika**: 50 - 50,000 PLN
- **Książki**: 5 - 500 PLN  
- **Odzież**: 10 - 5,000 PLN
- **Inna kategoria**: Odrzucona

---

## 🚫 System zabronionych słów (Ocena 3.5)

### **Jak działa walidacja zabronionych słów:**

```csharp
public async Task<List<string>> GetForbiddenWordsInNameAsync(string productName)
{
    if (string.IsNullOrWhiteSpace(productName))
        return new List<string>();

    // 1. Pobierz aktywne zabronione słowa z bazy
    var forbiddenWords = await _forbiddenWords
        .Find(fw => fw.IsActive)
        .Project(fw => fw.Word)
        .ToListAsync();

    var foundForbiddenWords = new List<string>();
    var productNameLower = productName.ToLowerInvariant();

    // 2. Sprawdź czy nazwa zawiera któreś z zabronionych słów
    foreach (var word in forbiddenWords)
    {
        if (productNameLower.Contains(word.ToLowerInvariant()))
        {
            foundForbiddenWords.Add(word);
        }
    }

    return foundForbiddenWords;
}
```

### **Przykładowe zabronione słowa:**
- `spam` - słowo związane ze spamem
- `fake` - słowo sugerujące fałszywość  
- `hack` - słowo związane z nielegalną działalnością
- `virus` - słowo związane z malware
- `illegal` - słowo sugerujące nielegalność

### **Komunikat przy naruszeniu:**
```json
{
  "message": "Naruszenie polityki nazywania produktów. Nazwa zawiera zabronione słowa: spam, fake",
  "forbiddenWords": ["spam", "fake"]
}
```

### **Gdzie jest sprawdzane:**
1. **POST /api/v1/products** - przy tworzeniu nowego produktu
2. **PUT /api/v1/products/{id}** - przy edycji istniejącego produktu

---

## 🧪 Jak testować aplikację

### 1. **Uruchomienie aplikacji**
```powershell
cd ZtpLab1
dotnet run
```
Aplikacja uruchomi się na `http://localhost:5048`

### 2. **Swagger UI** 
Otwórz `http://localhost:5048` w przeglądarce - automatycznie przekieruje na Swagger UI

### 3. **Testowanie CRUD produktów**

#### **Pobierz listę produktów:**
```
GET http://localhost:5048/api/v1/products
```
**Oczekiwany wynik:** Lista z Id i Name

#### **Pobierz szczegóły produktu:**
```
GET http://localhost:5048/api/v1/products/{id}
```
**Oczekiwany wynik:** Pełne dane produktu (w tym Quantity)

#### **Dodaj nowy produkt:**
```
POST http://localhost:5048/api/v1/products
Content-Type: application/json

{
  "name": "TestProduct123",
  "category": "elektronika",
  "price": 1500,
  "quantity": 10
}
```
**Oczekiwany wynik:** 201 Created

#### **Testuj walidację ceny:**
```
POST http://localhost:5048/api/v1/products
Content-Type: application/json

{
  "name": "TaniegoLaptopa",
  "category": "elektronika", 
  "price": 30,  // ZA MAŁO! Min = 50
  "quantity": 5
}
```
**Oczekiwany wynik:** 400 Bad Request z komunikatem o błędnej cenie

#### **Testuj zabronione słowa:**
```
POST http://localhost:5048/api/v1/products
Content-Type: application/json

{
  "name": "SpamProduct",  // Zawiera "spam"!
  "category": "elektronika",
  "price": 100,
  "quantity": 1
}
```
**Oczekiwany wynik:** 400 Bad Request z komunikatem o naruszeniu polityki

### 4. **Testowanie zabronionych słów**

#### **Pobierz listę zabronionych słów:**
```
GET http://localhost:5048/api/v1/forbidden-words
```

#### **Dodaj nowe zabronione słowo:**
```
POST http://localhost:5048/api/v1/forbidden-words
Content-Type: application/json

{
  "word": "test",
  "reason": "Słowo testowe",
  "isActive": true
}
```

### 5. **Automatyczny seed danych**
Przy starcie aplikacji automatycznie dodawane są przykładowe:
- **Produkty:** LaptopAcer, KoszulkaNike, HarryPotter, SmartfonSamsung, BluzaAdidas
- **Zabronione słowa:** spam, fake, hack, virus, illegal

---

## 🎯 Podsumowanie działania

### **Przepływ dodawania produktu:**

1. **Żądanie HTTP** → `POST /api/v1/products`
2. **Model binding** → ASP.NET Core tworzy obiekt Product
3. **Walidacja atrybutów** → Sprawdzenie Required, StringLength, RegularExpression
4. **Walidacja domenowa** → Sprawdzenie ceny według kategorii
5. **Walidacja zabronionych słów** → Sprawdzenie nazwy w bazie ForbiddenWords
6. **Zapis do MongoDB** → Attempt insert (może rzucić błąd unikalności)
7. **Odpowiedź HTTP** → 201 Created lub odpowiedni błąd

### **Gwarancje systemu:**
- ✅ **Unikalność nazw** - wymuszana przez MongoDB
- ✅ **Walidacja cen** - sprawdzana w kodzie aplikacji  
- ✅ **Zabronione słowa** - sprawdzane przy każdej operacji
- ✅ **Persistencja** - wszystko zapisane w bazie MongoDB
- ✅ **REST API** - zgodne z konwencjami

### **Spełnienie wymagań:**
- 🎯 **Ocena 3.0**: 100% spełnienia wymagań
- 🎯 **Ocena 3.5**: 100% spełnienia wymagań  
- 🚀 **Bonus**: Dodatkowe funkcjonalności (Swagger, pełne CRUD zabronionych słów)

**Aplikacja jest w pełni funkcjonalna i gotowa do obrony!** 🏆

---

## 📊 Historia Produktów (Ocena 4.5)

### **Śledzenie zmian produktów**

System automatycznie śledzi wszystkie zmiany produktów i umożliwia przeglądanie pełnej historii operacji.

### **Dostępne endpointy historii:**

#### **1. GET /api/v1/products/history** - Cała historia wszystkich produktów
```
GET http://localhost:5048/api/v1/products/history
```

**Co zwraca:**
- Pełną historię zmian **wszystkich produktów** w systemie
- Wpisy posortowane chronologicznie (najnowsze pierwsze)
- Szczegółowe informacje o każdej operacji (CREATE/UPDATE/DELETE)

**Przykład odpowiedzi:**
```json
{
  "success": true,
  "message": "Pobrano całą historię produktów pomyślnie.",
  "data": [
    {
      "id": "673c234567890abcdef12345",
      "productId": "507f1f77bcf86cd799439011", 
      "operationType": "UPDATE",
      "previousState": {
        "name": "LaptopDell",
        "category": "elektronika",
        "price": 3000,
        "quantity": 10
      },
      "currentState": {
        "name": "LaptopDell", 
        "category": "elektronika",
        "price": 3500,
        "quantity": 8
      },
      "changedAt": "2024-11-24T14:30:00Z",
      "changedFields": ["Price", "Quantity"]
    },
    {
      "id": "673c234567890abcdef12346",
      "productId": "507f1f77bcf86cd799439012",
      "operationType": "CREATE", 
      "previousState": null,
      "currentState": {
        "name": "KoszulkaNike",
        "category": "odzież",
        "price": 150,
        "quantity": 25
      },
      "changedAt": "2024-11-24T12:15:00Z",
      "changedFields": []
    }
  ]
}
```

#### **2. GET /api/v1/products/{id}/history** - Historia konkretnego produktu
```
GET http://localhost:5048/api/v1/products/507f1f77bcf86cd799439011/history
```

**Co zwraca:**
- Historię zmian **tylko dla wybranego produktu**
- Chronologiczną listę wszystkich operacji na tym produkcie
- Szczegóły poprzedniego i aktualnego stanu przy każdej zmianie

### **Kiedy historia jest zapisywana:**

1. **CREATE** - Przy dodawaniu nowego produktu
   - `previousState`: null
   - `currentState`: pełne dane nowego produktu
   - `changedFields`: pusta lista

2. **UPDATE** - Przy edycji istniejącego produktu  
   - `previousState`: stan przed zmianą
   - `currentState`: stan po zmianie
   - `changedFields`: lista pól które się zmieniły (np. ["Price", "Quantity"])

3. **DELETE** - Przy usuwaniu produktu
   - `previousState`: ostatni stan przed usunięciem
   - `currentState`: null
   - `changedFields`: pusta lista

### **Cechy systemu historii:**

✅ **Automatyczne** - historia zapisuje się bez interwencji użytkownika
✅ **Trwałe** - wszystkie dane przechowywane w MongoDB
✅ **Bezpieczne** - błędy historii nie przerywają głównych operacji  
✅ **Szczegółowe** - informacje o tym które pola zostały zmienione
✅ **Chronologiczne** - wpisy sortowane według daty (najnowsze pierwsze)
✅ **Kompletne** - pełny stan produktu przed i po zmianie

### **Testowanie historii:**

1. **Dodaj nowy produkt** → Sprawdź czy pojawił się wpis CREATE
2. **Edytuj produkt** → Sprawdź czy pojawił się wpis UPDATE z listą zmienionych pól
3. **Usuń produkt** → Sprawdź czy pojawił się wpis DELETE
4. **Pobierz całą historię** → Sprawdź czy widzisz operacje na wszystkich produktach
5. **Pobierz historię produktu** → Sprawdź czy widzisz tylko operacje na wybranym produkcie

### **Architektura historii:**

```
Operacja na produkcie (CREATE/UPDATE/DELETE)
        ↓
ProductService automatycznie wywołuje ProductHistoryService  
        ↓
ProductHistoryService tworzy wpis ProductHistory
        ↓ 
ProductHistoryRepository zapisuje w MongoDB (kolekcja ProductHistory)
        ↓
Endpointy /history udostępniają dane użytkownikowi
```

**Historia produktów jest w pełni zaimplementowana zgodnie z wymaganiami oceny 4.5!** 📊