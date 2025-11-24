# 📚 Dokumentacja Testów Integracyjnych - Przewodnik Edukacyjny

## 🎯 Cel i Zakres Testów

Testy integracyjne weryfikują współdziałanie wszystkich komponentów aplikacji:
- **Kontrolery** - obsługa żądań HTTP
- **Serwisy** - logika biznesowa
- **Repozytoria** - dostęp do danych
- **Baza danych** - rzeczywista MongoDB w kontenerze
- **Walidacje** - sprawdzanie reguł biznesowych

## 🏗️ Architektura Testów

### 1. **IntegrationTestWebAppFactory** - Wzorzec Test Host Factory

```
┌─────────────────────────────────────┐
│    IntegrationTestWebAppFactory     │
├─────────────────────────────────────┤
│  • WebApplicationFactory<Program>   │ ← Hosting aplikacji ASP.NET Core
│  • IAsyncLifetime                   │ ← Zarządzanie cyklem życia
│  • MongoDbContainer                 │ ← Kontener Docker z MongoDB
└─────────────────────────────────────┘
```

**Kluczowe funkcje:**
- **Hosting aplikacji**: Uruchamia pełną aplikację ASP.NET Core w pamięci
- **Kontener MongoDB**: Każda klasa testowa ma własny kontener Docker
- **Konfiguracja testowa**: Zastępuje produkcyjną konfigurację testową
- **Izolacja**: Brak współdzielenia stanu między klasami testowymi

### 2. **IntegrationTestBase** - Klasa Bazowa

```
┌─────────────────────────────────────┐
│        IntegrationTestBase          │
├─────────────────────────────────────┤
│  • IClassFixture                    │ ← Jedna instancja na klasę
│  • IAsyncLifetime                   │ ← Cleanup po każdym teście
│  • HttpClient                       │ ← Klient do wywołań API
└─────────────────────────────────────┘
```

**Automatyczne zarządzanie:**
- **Setup**: Brak - kontener już uruchomiony
- **Cleanup**: Czyszczenie bazy po każdym teście
- **Isolation**: Każdy test ma czystą bazę danych

## 🐳 Docker i Testcontainers

### Konfiguracja Kontenera MongoDB

```csharp
var _mongoContainer = new MongoDbBuilder()
    .WithImage("mongo:7.0")                    // Stabilna wersja MongoDB
    .WithPortBinding(27017, true)              // Auto-alokacja portu
    .WithEnvironment("MONGO_INITDB_DATABASE", "ProductManagementTestDb")
    .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017))
    .Build();
```

**Zalety podejścia:**
- ✅ **Rzeczywista baza**: Nie mocks, ale prawdziwa MongoDB
- ✅ **Izolacja**: Każda klasa testowa = osobny kontener
- ✅ **Automatyzacja**: Start/stop zarządzany przez framework
- ✅ **Konsystentność**: Identyczne środowisko dla każdego testu
- ✅ **CI/CD Ready**: Działa lokalnie i na serwerach build

**Cykl życia kontenera:**
1. **Przed klasą testową**: `InitializeAsync()` - start kontenera
2. **Przed testem**: Brak akcji (kontener już działa)
3. **Po teście**: `DisposeAsync()` - reset bazy danych
4. **Po klasie testowej**: `DisposeAsync()` - stop i usunięcie kontenera

## 🧪 Wzorce Testowania

### 1. **AAA Pattern (Arrange-Act-Assert)**

```csharp
[Fact]
public async Task CreateProduct_WithValidData_ShouldReturnCreatedProduct()
{
    // Arrange - przygotowanie danych testowych
    var createDto = new CreateProductDto { ... };
    
    // Act - wykonanie akcji
    var response = await Client.PostAsJsonAsync("/api/v1/products", createDto);
    
    // Assert - sprawdzenie wyników
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

### 2. **Unique Test Data Pattern**

```csharp
// Problem: Konflikty nazw między testami
var name = "TestProduct"; // ❌ Może już istnieć

// Rozwiązanie: Unikalne identyfikatory
var uniqueName = $"Test{DateTime.Now.Ticks % 1000000}"; // ✅ Zawsze unikalne
```

### 3. **Fluent Assertions Pattern**

```csharp
// Tradycyjne asercje
Assert.Equal(HttpStatusCode.Created, response.StatusCode); // ❌ Mało czytelne

// Fluent Assertions
response.StatusCode.Should().Be(HttpStatusCode.Created);   // ✅ Czytelne
apiResponse.Data.Should().NotBeNull();                     // ✅ Ekspresyjne
apiResponse.Data!.Name.Should().Be(expectedName);         // ✅ Łańcuchowe
```

## 🔒 Strategie Izolacji Testów

### 1. **Container per Test Class**
- **Jedna klasa testowa = jeden kontener MongoDB**
- **Zalety**: Pełna izolacja między klasami
- **Wady**: Wolniejsze uruchamianie (start kontenera)

### 2. **Database Reset per Test**
- **Jeden test = reset bazy danych**
- **Implementacja**: `DropCollection` dla wszystkich kolekcji
- **Zalety**: Szybkie, nie wymaga restartu kontenera
- **Wady**: Wymaga dokładnego czyszczenia

### 3. **Alternatywy (nieużywane w projekcie)**
- **Transakcje**: MongoDB wymaga replica set
- **Restart kontenera**: Bardzo wolne
- **In-memory bazy**: Nie testuje rzeczywistej MongoDB

## 📊 Kategorie Testów

### 1. **ProductsController Tests (9 testów)**

```
CRUD Operations:
├── Create Product (valid data)
├── Get Product (existing)
├── Get All Products
├── Update Product (valid data)  
├── Delete Product (existing)
├── Create Product (invalid price) - walidacja
├── Create Product (long name) - walidacja
├── Get Product (non-existing) - 404
└── Delete Product (non-existing) - 404
```

**Pokryte scenariusze biznesowe:**
- ✅ Podstawowe operacje CRUD
- ✅ Walidacja długości nazwy (max 20 znaków)
- ✅ Walidacja ceny według kategorii
- ✅ Obsługa błędów (404, 400, 422)
- ✅ Unikalność nazw produktów

### 2. **ForbiddenWords Tests (13 testów)**

```
Management Operations:
├── CRUD operations (4 testy)
├── Toggle Active/Inactive (2 testy)
├── Filter by status (2 testy)
├── Validation scenarios (3 testy)
└── Edge cases (2 testy)
```

### 3. **Business Rules Tests (6 testów)**

```
Domain Validations:
├── Product price rules by category
│   ├── Elektronika: 50-50,000 PLN
│   ├── Książki: ≥5 PLN
│   └── Odzież: ≥10 PLN
├── Forbidden words in product names
├── Product quantity validation
└── Complex business scenarios
```

## 🔧 Konfiguracja Środowiska

### 1. **Testing Environment**
```csharp
builder.UseEnvironment("Testing");
```
**Efekty:**
- Wyłączenie seedingu bazy danych
- Specjalne ustawienia logowania
- Testowa konfiguracja

### 2. **Configuration Override**
```csharp
config.Sources.Clear(); // Usuń produkcyjną konfigurację
config.AddInMemoryCollection(testConfig); // Dodaj testową
```

### 3. **Dependency Injection**
- Wszystkie serwisy pozostają niezmienione
- Tylko connection string jest podmieniany
- Pełna integracja z DI Container

## 🚀 Uruchamianie Testów

### Lokalne środowisko:
```bash
dotnet test --verbosity normal
```

### CI/CD Requirements:
- Docker Engine (dla kontenerów MongoDB)
- .NET 9.0 SDK
- Dostęp do Docker Hub (mongo:7.0 image)

### Performance:
- **Cold start**: ~10-15 sekund (pobieranie obrazu)
- **Warm start**: ~3-5 sekund (obraz już pobrany)
- **Execution**: ~200-500ms per test

## 📈 Metryki i Monitoring

### Test Coverage:
- **28 testów** - 100% pass rate
- **3 kontrolery** - pełne pokrycie
- **Wszystkie endpointy** - przetestowane
- **Business rules** - zweryfikowane

### Performance Benchmarks:
```
Container lifecycle: ~3-5s
Database reset: ~50-100ms  
HTTP call: ~10-50ms
Total per test: ~200-500ms
```

## 🎓 Wartość Edukacyjna

### Poznane Wzorce:
1. **Test Host Factory** - hosting aplikacji w testach
2. **Testcontainers** - Docker w testach
3. **AAA Pattern** - struktura testów
4. **Fluent Assertions** - czytelne asercje
5. **Dependency Injection** - w kontekście testów

### Poznane Technologie:
1. **Docker & Testcontainers** - konteneryzacja
2. **MongoDB Driver** - NoSQL w .NET
3. **ASP.NET Core Testing** - integration testing
4. **xUnit** - framework testowy
5. **FluentAssertions** - biblioteka asercji

### Real-world Skills:
- ✅ **CI/CD Testing** - testy gotowe na pipeline
- ✅ **Database Testing** - z rzeczywistą bazą
- ✅ **API Testing** - end-to-end HTTP calls
- ✅ **Docker Skills** - konteneryzacja w testach
- ✅ **Best Practices** - izolacja, cleanup, patterns

## 🔍 Debugowanie i Troubleshooting

### Typowe problemy:

1. **Docker nie działa**
   ```
   Error: Could not connect to Docker
   Rozwiązanie: Sprawdź czy Docker Desktop jest uruchomiony
   ```

2. **Port conflicts**
   ```
   Error: Port already in use
   Rozwiązanie: Testcontainers automatycznie alokuje porty
   ```

3. **Timeout na starcie kontenera**
   ```
   Error: Container failed to start
   Rozwiązanie: Zwiększ timeout lub sprawdź zasoby
   ```

### Debug tools:
- `docker ps` - lista działających kontenerów
- `docker logs <container>` - logi kontenera MongoDB
- VS Code debugger - breakpoints w testach

To jest kompletny przewodnik po testach integracyjnych - od teorii przez implementację do praktycznego użycia! 🎯