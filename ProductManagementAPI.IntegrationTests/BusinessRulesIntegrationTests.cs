using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProductManagementAPI.IntegrationTests.Infrastructure;
using Xunit;
using ZtpLab1.DataTransferObjects;
using ZtpLab1.Models;

namespace ProductManagementAPI.IntegrationTests;

/// <summary>
/// Testy integracyjne dla Business Rules i Domain Requirements.
/// Testują zgodność całego systemu z wymaganiami domenowymi,
/// szczególnie interakcje między produktami a zabronionymi słowami.
/// </summary>
public class BusinessRulesIntegrationTests : IntegrationTestBase
{
    public BusinessRulesIntegrationTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ProductCreation_WithMultipleForbiddenWords_ShouldValidateAllWords()
    {
        // Arrange - Dodaj kilka zabronionych słów
        var forbiddenWords = new[]
        {
            new CreateForbiddenWordDto { Word = "spam", Reason = "Unwanted content", IsActive = true },
            new CreateForbiddenWordDto { Word = "fake", Reason = "Misleading content", IsActive = true },
            new CreateForbiddenWordDto { Word = "scam", Reason = "Fraudulent content", IsActive = true }
        };

        foreach (var word in forbiddenWords)
        {
            await Client.PostAsJsonAsync("/api/v1/forbidden-words", word);
        }

        // Test Case 1: Nazwa produktu z jednym zabronionym słowem
        var productWithSpam = new CreateProductDto
        {
            Name = "SuperSpam", // Zawiera "spam" - krótka nazwa
            Category = "książki",
            Price = 19.99m,
            Quantity = 5
        };

        var spamResponse = await Client.PostAsJsonAsync("/api/v1/products", productWithSpam);
        spamResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        // Test Case 2: Nazwa produktu z wieloma zabronionymi słowami
        var productWithMultipleForbidden = new CreateProductDto
        {
            Name = "FakeSpamScam", // Zawiera wszystkie trzy zabronione słowa - krótka nazwa
            Category = "odzież",
            Price = 15.99m,
            Quantity = 1
        };

        var multipleResponse = await Client.PostAsJsonAsync("/api/v1/products", productWithMultipleForbidden);
        multipleResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        
        var apiResponse = await multipleResponse.Content.ReadFromJsonAsync<ApiResponse>();
        apiResponse!.ValidationErrors.Should().NotBeNull();
        apiResponse.ValidationErrors.Should().Contain(error => error.Message.Contains("zabronione słowa"));

        // Test Case 3: Produkt bez zabronionych słów powinien przejść
        var baseName = (DateTime.Now.Ticks % 100000).ToString();
        var validProduct = new CreateProductDto
        {
            Name = $"Valid{baseName}", // Krótka unikalna nazwa
            Category = "elektronika",
            Price = 99.99m,
            Quantity = 10
        };

        var validResponse = await Client.PostAsJsonAsync("/api/v1/products", validProduct);
        validResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task ProductUpdate_WithForbiddenWordValidation_ShouldEnforceBusinessRules()
    {
        // Arrange - Utwórz prawidłowy produkt
        var baseName = (DateTime.Now.Ticks % 100000).ToString();
        var createDto = new CreateProductDto
        {
            Name = $"Orig{baseName}", // Krótka unikalna nazwa
            Category = "elektronika",
            Price = 55.00m,
            Quantity = 5
        };

        var createResponse = await Client.PostAsJsonAsync("/api/v1/products", createDto);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Product>>();
        var productId = createdProduct!.Data!.Id;

        // Dodaj zabronione słowo
        var forbiddenWord = new CreateForbiddenWordDto
        {
            Word = "illegal",
            Reason = "Prohibited content",
            IsActive = true
        };
        await Client.PostAsJsonAsync("/api/v1/forbidden-words", forbiddenWord);

        // Act & Assert - Próba aktualizacji na nazwę z zabronionym słowem
        var invalidUpdateDto = new UpdateProductDto
        {
            Name = "IllegalUpdate", // Zawiera zabronione słowo - krótka nazwa
            Category = "książki",
            Price = 75.00m,
            Quantity = 8
        };

        var updateResponse = await Client.PutAsJsonAsync($"/api/v1/products/{productId}", invalidUpdateDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        // Sprawdź, że oryginalny produkt pozostał niezmieniony
        var getResponse = await Client.GetAsync($"/api/v1/products/{productId}");
        var originalProduct = await getResponse.Content.ReadFromJsonAsync<ApiResponse<Product>>();
        originalProduct!.Data!.Name.Should().Be(createDto.Name); // Nazwa nie została zmieniona
    }

    [Fact]
    public async Task ForbiddenWordStatusToggle_ShouldAffectProductValidation()
    {
        // Arrange - Dodaj zabronione słowo
        var uniqueWord = $"restr{DateTime.Now.Ticks % 100000}";
        var forbiddenWordDto = new CreateForbiddenWordDto
        {
            Word = uniqueWord,
            Reason = "Content restriction",
            IsActive = true
        };

        var forbiddenWordResponse = await Client.PostAsJsonAsync("/api/v1/forbidden-words", forbiddenWordDto);
        var forbiddenWord = await forbiddenWordResponse.Content.ReadFromJsonAsync<ApiResponse<ForbiddenWord>>();
        var wordId = forbiddenWord!.Data!.Id;

        // Test 1: Gdy słowo jest aktywne, produkt z tym słowem powinien być odrzucony
        var productWithRestrictedWord = new CreateProductDto
        {
            Name = uniqueWord, // Nazwa zawiera unikalne zabronione słowo
            Category = "elektronika",
            Price = 55.00m,
            Quantity = 3
        };

        var rejectedResponse = await Client.PostAsJsonAsync("/api/v1/products", productWithRestrictedWord);
        rejectedResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        // Act - Wyłącz zabronione słowo
        var toggleResponse = await Client.PatchAsync($"/api/v1/forbidden-words/{wordId}/toggle", null);
        toggleResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Test 2: Gdy słowo jest nieaktywne, produkt z tym słowem powinien przejść
        var acceptedResponse = await Client.PostAsJsonAsync("/api/v1/products", productWithRestrictedWord);
        acceptedResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Włącz ponownie zabronione słowo
        await Client.PatchAsync($"/api/v1/forbidden-words/{wordId}/toggle", null);

        // Test 3: Po ponownym włączeniu, nowe produkty z tym słowem powinny być odrzucone
        var productWithRestrictedWord2 = new CreateProductDto
        {
            Name = $"{uniqueWord}2", // Zawiera unikalne zabronione słowo
            Category = "książki",
            Price = 30.00m,
            Quantity = 2
        };

        var rejectedResponse2 = await Client.PostAsJsonAsync("/api/v1/products", productWithRestrictedWord2);
        rejectedResponse2.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ProductPriceValidation_ShouldEnforceDomainRules()
    {
        // Test Case 1: Cena zerowa
        var testId1 = (DateTime.Now.Ticks % 10000).ToString();
        var zeroPrice = new CreateProductDto
        {
            Name = $"Zero{testId1}",
            Category = "elektronika",
            Price = 0.00m,
            Quantity = 5
        };

        var zeroResponse = await Client.PostAsJsonAsync("/api/v1/products", zeroPrice);
        zeroResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Test Case 2: Cena ujemna
        var testId2 = (DateTime.Now.Ticks % 10000).ToString();
        var negativePrice = new CreateProductDto
        {
            Name = $"Neg{testId2}", 
            Category = "książki",
            Price = -10.00m,
            Quantity = 5
        };

        var negativeResponse = await Client.PostAsJsonAsync("/api/v1/products", negativePrice);
        negativeResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Test Case 3: Minimalna prawidłowa cena
        var testId3 = (DateTime.Now.Ticks % 10000).ToString();
        var minValidPrice = new CreateProductDto
        {
            Name = $"V{testId3}", // Bardzo krótka nazwa
            Category = "odzież", 
            Price = 15.00m, // Prawidłowa cena dla odzieży
            Quantity = 5
        };

        var validResponse = await Client.PostAsJsonAsync("/api/v1/products", minValidPrice);
        validResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Test Case 4: Wysoka ale dozwolona cena (max 50000 PLN dla elektroniki)
        var testId4 = (DateTime.Now.Ticks % 10000).ToString();
        var highPrice = new CreateProductDto
        {
            Name = $"ExpensiveProd{testId4}", // Dłuższa unikalna nazwa
            Category = "elektronika",
            Price = 49999.99m, // Maksymalna dozwolona cena
            Quantity = 1
        };

        var expensiveResponse = await Client.PostAsJsonAsync("/api/v1/products", highPrice);
        expensiveResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task ProductQuantityValidation_ShouldEnforceDomainRules()
    {
        // Test Case 1: Ujemna ilość
        var testId1 = (DateTime.Now.Ticks % 10000).ToString();
        var negativeQuantity = new CreateProductDto
        {
            Name = $"NegQ{testId1}",
            Category = "książki",
            Price = 10.00m,
            Quantity = -5
        };

        var negativeResponse = await Client.PostAsJsonAsync("/api/v1/products", negativeQuantity);
        negativeResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Test Case 2: Zerowa ilość (powinna być dozwolona - produkt może być chwilowo niedostępny)
        var testId2 = (DateTime.Now.Ticks % 10000).ToString();
        var zeroQuantity = new CreateProductDto
        {
            Name = $"Zero{testId2}",
            Category = "odzież",
            Price = 15.00m,
            Quantity = 0
        };

        var zeroResponse = await Client.PostAsJsonAsync("/api/v1/products", zeroQuantity);
        zeroResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Test Case 3: Duża ilość
        var testId3 = (DateTime.Now.Ticks % 10000).ToString();
        var largeQuantity = new CreateProductDto
        {
            Name = $"Large{testId3}",
            Category = "elektronika",
            Price = 55.00m,
            Quantity = 999999
        };

        var largeResponse = await Client.PostAsJsonAsync("/api/v1/products", largeQuantity);
        largeResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task ComplexDomainScenario_ProductWithForbiddenWordsInDifferentCases_ShouldBeValidated()
    {
        // Arrange - Dodaj zabronione słowa w różnych przypadkach
        var forbiddenWords = new[]
        {
            new CreateForbiddenWordDto { Word = "bad", Reason = "Inappropriate", IsActive = true },
            new CreateForbiddenWordDto { Word = "TERRIBLE", Reason = "Very inappropriate", IsActive = true },
            new CreateForbiddenWordDto { Word = "Awful", Reason = "Poor quality indicator", IsActive = true }
        };

        foreach (var word in forbiddenWords)
        {
            await Client.PostAsJsonAsync("/api/v1/forbidden-words", word);
        }

        // Test różnych przypadków liter w nazwie produktu
        var uniqueId = DateTime.Now.Ticks % 10000;
        var testCases = new[]
        {
            new { Name = "BADProd", ShouldFail = true }, // Wielkie litery - zawiera "bad"
            new { Name = "terribleitem", ShouldFail = true }, // Małe litery - zawiera "terrible"
            new { Name = "AwfulThing", ShouldFail = true }, // Mieszane przypadki - zawiera "Awful"
            new { Name = $"Good{uniqueId}", ShouldFail = false } // Bez zabronionych słów - unikalna nazwa
        };

        foreach (var testCase in testCases)
        {
            var product = new CreateProductDto
            {
                Name = testCase.Name,
                Category = "elektronika",
                Price = 55.00m,
                Quantity = 5
            };

            var response = await Client.PostAsJsonAsync("/api/v1/products", product);
            
            if (testCase.ShouldFail)
            {
                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity, 
                    $"Product with name '{testCase.Name}' should be rejected");
            }
            else
            {
                response.StatusCode.Should().Be(HttpStatusCode.Created,
                    $"Product with name '{testCase.Name}' should be accepted");
            }
        }
    }
}